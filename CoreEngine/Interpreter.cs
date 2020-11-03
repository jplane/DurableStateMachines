using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CoreEngine
{
    public class Interpreter
    {
        private readonly StateChart _statechart;
        private readonly ExecutionContext _executionContext;

        public Interpreter(XDocument xml)
        {
            _statechart = new StateChart(xml);
            _executionContext = new ExecutionContext();
        }

        public bool IsRunning => _executionContext.IsRunning;

        public void Interpret()
        {
            if (_statechart.IsEarlyBinding)
            {
                _executionContext.DataModel.Init(_statechart);
            }

            _executionContext.IsRunning = true;

            ExecuteGlobalScriptElement();

            var root = _statechart.GetState("scxml_root");

            EnterStates(new List<Transition>(new []{ root.GetInitialStateTransition() }));

            DoEventLoop();
        }

        private void DoEventLoop()
        {
            while (_executionContext.IsRunning)
            {
                OrderedSet<Transition> enabledTransitions = null;

                var macrostepDone = false;

                while (_executionContext.IsRunning && ! macrostepDone)
                {
                    enabledTransitions = SelectEventlessTransitions();

                    if (enabledTransitions.IsEmpty())
                    {
                        if (_executionContext.InternalQueue.Count == 0)
                        {
                            macrostepDone = true;
                        }
                        else if (_executionContext.InternalQueue.TryDequeue(out Event internalEvent))
                        {
                            _executionContext.DataModel["_event"] = internalEvent;
                            
                            enabledTransitions = SelectTransitions(internalEvent);
                        }
                    }

                    if (! enabledTransitions.IsEmpty())
                    {
                        Microstep(enabledTransitions.ToList());
                    }
                }

                if (! _executionContext.IsRunning)
                {
                    break;
                }

                foreach (var state in _executionContext.StatesToInvoke.ToList())
                {
                    state.Invoke(_executionContext, _statechart);
                }

                _executionContext.StatesToInvoke.Clear();

                if (_executionContext.InternalQueue.Count > 0)
                {
                    continue;
                }

                Event externalEvent;

                while (! _executionContext.ExternalQueue.TryDequeue(out externalEvent))
                {
                    Thread.Sleep(1000);
                }

                if (externalEvent.IsCancel)
                {
                    _executionContext.IsRunning = false;
                    continue;
                }

                _executionContext.DataModel["_event"] = externalEvent;

                foreach (var state in _executionContext.Configuration.ToList())
                {
                    foreach (var invoke in state.Invokes)
                    {
                        invoke.ProcessExternalEvent(_executionContext, externalEvent);
                    }
                }

                enabledTransitions = SelectTransitions(externalEvent);

                if (! enabledTransitions.IsEmpty())
                {
                    Microstep(enabledTransitions.ToList());
                }
            }

            var statesToExit = _executionContext.Configuration.ToList();

            statesToExit.Sort(_statechart.CompareReverseDocumentOrder);

            foreach (var state in statesToExit)
            {
                state.Exit(_executionContext, _statechart);

                if (state.IsFinalState)
                {
                    var parent = state.GetParent(_statechart);

                    if (parent.IsScxmlRoot)
                    {
                        ReturnDoneEvent(state);
                    }
                }
            }
        }

        private void ReturnDoneEvent(State state)
        {
            // The implementation of returnDoneEvent is platform-dependent, but if this session is the result of an <invoke> in another SCXML session, 
            //  returnDoneEvent will cause the event done.invoke.<id> to be placed in the external event queue of that session, where <id> is the id 
            //  generated in that session when the <invoke> was executed.
        }

        private void Microstep(List<Transition> enabledTransitions)
        {
            ExitStates(enabledTransitions);
            
            foreach (var transition in enabledTransitions)
            {
                transition.ExecuteContent(_executionContext, _statechart);
            }

            EnterStates(enabledTransitions);
        }

        private void ExitStates(List<Transition> enabledTransitions)
        {
            var exitSet = ComputeExitSet(enabledTransitions);

            var statesToExit = exitSet.ToList();

            foreach (var state in statesToExit)
            {
                _executionContext.StatesToInvoke.Delete(state);
            }

            statesToExit.Sort(_statechart.CompareReverseDocumentOrder);

            foreach (var state in statesToExit)
            {
                state.RecordHistory(_executionContext, _statechart);

                state.Exit(_executionContext, _statechart);
            }
        }

        private OrderedSet<State> ComputeExitSet(List<Transition> transitions)
        {
            var statesToExit = new OrderedSet<State>();

            foreach (var transition in transitions)
            {
                if (transition.HasTargets)
                {
                    var domain = transition.GetTransitionDomain(_executionContext, _statechart);

                    foreach (var state in _executionContext.Configuration.ToList())
                    {
                        if (state.IsDescendent(domain))
                        {
                            statesToExit.Add(state);
                        }
                    }
                }
            }

            return statesToExit;
        }

        private OrderedSet<Transition> SelectTransitions(Event evt)
        {
            return SelectTransitions(transition => transition.MatchesEvent(evt) &&
                                                   transition.EvaluateCondition(_executionContext, _statechart));
        }

        private OrderedSet<Transition> SelectEventlessTransitions()
        {
            return SelectTransitions(transition => !transition.HasEvent &&
                                                   transition.EvaluateCondition(_executionContext, _statechart));
        }

        private OrderedSet<Transition> SelectTransitions(Func<Transition, bool> predicate)
        {
            var enabledTransitions = new OrderedSet<Transition>();

            var atomicStates = _executionContext.Configuration.ToList().Filter(s => s.IsAtomic);

            atomicStates.Sort(_statechart.CompareDocumentOrder);

            foreach (var state in atomicStates)
            {
                var all = new List<State>();

                all.Append(state);

                foreach (var anc in state.GetProperAncestors(_statechart))
                {
                    all.Append(anc);
                }

                foreach (var s in all)
                {
                    foreach (var transition in s.Transitions)
                    {
                        if (predicate(transition))
                        {
                            enabledTransitions.Add(transition);
                            goto nextAtomicState;
                        }
                    }
                }

                nextAtomicState:
                    continue;
            }

            enabledTransitions = RemoveConflictingTransitions(enabledTransitions);

            return enabledTransitions;
        }

        private OrderedSet<Transition> RemoveConflictingTransitions(OrderedSet<Transition> enabledTransitions)
        {
            var filteredTransitions = new OrderedSet<Transition>();

            foreach (var transition1 in enabledTransitions.ToList())
            {
                var t1Preempted = false;

                var transitionsToRemove = new OrderedSet<Transition>();

                foreach (var transition2 in filteredTransitions.ToList())
                {
                    var exitSet1 = ComputeExitSet(List<Transition>.Create(transition1));

                    var exitSet2 = ComputeExitSet(List<Transition>.Create(transition2));

                    if (exitSet1.HasIntersection(exitSet2))
                    {
                        if (transition1.Source.IsDescendent(transition2.Source))
                        {
                            transitionsToRemove.Add(transition2);
                        }
                        else
                        {
                            t1Preempted = true;
                            break;
                        }
                    }
                }

                if (! t1Preempted)
                {
                    foreach (var transition3 in transitionsToRemove.ToList())
                    {
                        filteredTransitions.Delete(transition3);
                    }

                    filteredTransitions.Add(transition1);
                }
            }

            return filteredTransitions;
        }

        private void ExecuteGlobalScriptElement()
        {
        }

        private void EnterStates(List<Transition> enabledTransitions)
        {
            var statesToEnter = new OrderedSet<State>();

            var statesForDefaultEntry = new OrderedSet<State>();

            var defaultHistoryContent = new SCG.Dictionary<string, OrderedSet<ExecutableContent>>();

            ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

            var toEnter = statesToEnter.ToList();

            toEnter.Sort(_statechart.CompareDocumentOrder);

            foreach (var state in toEnter)
            {
                state.Enter(_executionContext, _statechart, statesForDefaultEntry, defaultHistoryContent);
            }
        }

        private void ComputeEntrySet(List<Transition> enabledTransitions,
                                     OrderedSet<State> statesToEnter,
                                     OrderedSet<State> statesForDefaultEntry,
                                     SCG.Dictionary<string, OrderedSet<ExecutableContent>> defaultHistoryContent)
        {
            foreach (var transition in enabledTransitions)
            {
                foreach (var state in transition.GetTargetStates(_statechart))
                {
                    AddDescendentStatesToEnter(state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }

                var ancestor = transition.GetTransitionDomain(_executionContext, _statechart);

                var effectiveTargetStates = transition.GetEffectiveTargetStates(_executionContext, _statechart);

                foreach (var state in effectiveTargetStates.ToList())
                {
                    AddAncestorStatesToEnter(state, ancestor, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }
            }
        }

        private void AddAncestorStatesToEnter(State state,
                                              State ancestor,
                                              OrderedSet<State> statesToEnter,
                                              OrderedSet<State> statesForDefaultEntry,
                                              SCG.Dictionary<string, OrderedSet<ExecutableContent>> defaultHistoryContent)
        {
            var ancestors = state.GetProperAncestors(_statechart, ancestor);

            foreach (var anc in ancestors)
            {
                statesToEnter.Add(anc);

                if (anc.IsParallelState)
                {
                    var childStates = anc.GetChildStates(_statechart);

                    foreach (var child in childStates)
                    {
                        if (! statesToEnter.Some(s => s.IsDescendent(child)))
                        {
                            AddDescendentStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }

        private void AddDescendentStatesToEnter(State state,
                                                OrderedSet<State> statesToEnter,
                                                OrderedSet<State> statesForDefaultEntry,
                                                SCG.Dictionary<string, OrderedSet<ExecutableContent>> defaultHistoryContent)
        {
            if (state.IsHistoryState)
            {
                var parent = state.GetParent(_statechart);

                if (_executionContext.HistoryValue.TryGetValue(state.Id, out List<State> states))
                {
                    foreach (var s in states)
                    {
                        AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in states)
                    {
                        AddAncestorStatesToEnter(s, parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else
                {
                    defaultHistoryContent[parent.Id] = OrderedSet<ExecutableContent>.Union(state.Transitions.Select(t => t.Content));

                    var targetStates = OrderedSet<State>.Union(state.Transitions.Select(t => t.GetTargetStates(_statechart))).ToList();

                    foreach (var s in targetStates)
                    {
                        AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in targetStates)
                    {
                        AddAncestorStatesToEnter(s, parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
            }
            else
            {
                statesToEnter.Add(state);

                if (state.IsCompoundState)
                {
                    statesForDefaultEntry.Add(state);

                    var initialTransition = state.GetInitialStateTransition();

                    var targetStates = initialTransition.GetTargetStates(_statechart);

                    foreach (var s in targetStates)
                    {
                        AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in targetStates)
                    {
                        AddAncestorStatesToEnter(s, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else if (state.IsParallelState)
                {
                    var childStates = state.GetChildStates(_statechart);

                    foreach (var child in childStates)
                    {
                        if (! statesToEnter.Some(s => s.IsDescendent(child)))
                        {
                            AddDescendentStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }
    }
}
