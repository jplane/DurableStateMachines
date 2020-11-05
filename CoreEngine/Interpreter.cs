using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using CoreEngine.Model;
using CoreEngine.Model.States;
using CoreEngine.Model.Execution;

namespace CoreEngine
{
    public class Interpreter
    {
        private readonly RootState _root;
        private readonly ExecutionContext _executionContext;

        public Interpreter(XDocument xml)
        {
            _root = new RootState(xml.Root);
            _executionContext = new ExecutionContext();
        }

        public ExecutionContext Context => _executionContext;

        public void Run()
        {
            if (_root.Binding == Databinding.Early)
            {
                _root.InitDatamodel(_executionContext, true);
            }

            _executionContext.IsRunning = true;

            _root.ExecuteScript(_executionContext);

            EnterStates(new List<Transition>(new []{ _root.GetInitialStateTransition() }));

            DoEventLoop();
        }

        private void DoEventLoop()
        {
            while (_executionContext.IsRunning)
            {
                Set<Transition> enabledTransitions = null;

                var macrostepDone = false;

                while (_executionContext.IsRunning && ! macrostepDone)
                {
                    enabledTransitions = SelectEventlessTransitions();

                    if (enabledTransitions.IsEmpty())
                    {
                        var internalEvent = _executionContext.DequeueInternal();

                        if (internalEvent == null)
                        {
                            macrostepDone = true;
                        }
                        else
                        {
                            enabledTransitions = SelectTransitions(internalEvent);
                        }
                    }

                    if (! enabledTransitions.IsEmpty())
                    {
                        Microstep(enabledTransitions);
                    }
                }

                if (! _executionContext.IsRunning)
                {
                    break;
                }

                foreach (var state in _executionContext.StatesToInvoke.Sort(State.GetXObject))
                {
                    state.Invoke(_executionContext, _root);
                }

                _executionContext.StatesToInvoke.Clear();

                if (_executionContext.HasInternalEvents)
                {
                    continue;
                }

                var externalEvent = _executionContext.DequeueExternal();

                if (externalEvent.IsCancel)
                {
                    _executionContext.IsRunning = false;
                    continue;
                }

                foreach (var state in _executionContext.Configuration)
                {
                    state.ProcessExternalEvent(_executionContext, externalEvent);
                }

                enabledTransitions = SelectTransitions(externalEvent);

                if (! enabledTransitions.IsEmpty())
                {
                    Microstep(enabledTransitions);
                }
            }

            foreach (var state in _executionContext.Configuration.Sort(State.GetXObject, true))
            {
                state.Exit(_executionContext);

                if (state.IsFinalState)
                {
                    if (state.Parent.IsScxmlRoot)
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

        private void Microstep(IEnumerable<Transition> enabledTransitions)
        {
            ExitStates(enabledTransitions);
            
            foreach (var transition in enabledTransitions)
            {
                transition.ExecuteContent(_executionContext);
            }

            EnterStates(enabledTransitions);
        }

        private void ExitStates(IEnumerable<Transition> enabledTransitions)
        {
            var exitSet = ComputeExitSet(enabledTransitions);

            foreach (var state in exitSet)
            {
                _executionContext.StatesToInvoke.Remove(state);
            }

            foreach (var state in exitSet.Sort(State.GetXObject, true))
            {
                state.RecordHistory(_executionContext, _root);

                state.Exit(_executionContext);
            }
        }

        private Set<State> ComputeExitSet(IEnumerable<Transition> transitions)
        {
            var statesToExit = new Set<State>();

            foreach (var transition in transitions)
            {
                if (transition.HasTargets)
                {
                    var domain = transition.GetTransitionDomain(_executionContext, _root);

                    foreach (var state in _executionContext.Configuration)
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

        private Set<Transition> SelectTransitions(Event evt)
        {
            return SelectTransitions(transition => transition.MatchesEvent(evt) &&
                                                   transition.EvaluateCondition(_executionContext));
        }

        private Set<Transition> SelectEventlessTransitions()
        {
            return SelectTransitions(transition => !transition.HasEvent &&
                                                   transition.EvaluateCondition(_executionContext));
        }

        private Set<Transition> SelectTransitions(Func<Transition, bool> predicate)
        {
            var enabledTransitions = new Set<Transition>();

            var atomicStates = _executionContext.Configuration
                                                .Sort(State.GetXObject)
                                                .Where(s => s.IsAtomic);

            foreach (var state in atomicStates)
            {
                var all = new List<State>();

                all.Add(state);

                foreach (var anc in state.GetProperAncestors(_root))
                {
                    all.Add(anc);
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

        private Set<Transition> RemoveConflictingTransitions(IEnumerable<Transition> enabledTransitions)
        {
            var filteredTransitions = new Set<Transition>();

            foreach (var transition1 in enabledTransitions)
            {
                var t1Preempted = false;

                var transitionsToRemove = new Set<Transition>();

                foreach (var transition2 in filteredTransitions)
                {
                    var exitSet1 = ComputeExitSet(new List<Transition> { transition1 });

                    var exitSet2 = ComputeExitSet(new List<Transition> { transition2 });

                    if (exitSet1.HasIntersection(exitSet2))
                    {
                        if (transition1.IsSourceDescendent(transition2))
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
                    foreach (var transition3 in transitionsToRemove)
                    {
                        filteredTransitions.Remove(transition3);
                    }

                    filteredTransitions.Add(transition1);
                }
            }

            return filteredTransitions;
        }

        private void EnterStates(IEnumerable<Transition> enabledTransitions)
        {
            var statesToEnter = new Set<State>();

            var statesForDefaultEntry = new Set<State>();

            var defaultHistoryContent = new Dictionary<string, Set<ExecutableContent>>();

            ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

            foreach (var state in statesToEnter.Sort(State.GetXObject))
            {
                state.Enter(_executionContext, _root, statesForDefaultEntry, defaultHistoryContent);
            }
        }

        private void ComputeEntrySet(IEnumerable<Transition> enabledTransitions,
                                     Set<State> statesToEnter,
                                     Set<State> statesForDefaultEntry,
                                     Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            foreach (var transition in enabledTransitions)
            {
                foreach (var state in transition.GetTargetStates(_root))
                {
                    AddDescendentStatesToEnter(state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }

                var ancestor = transition.GetTransitionDomain(_executionContext, _root);

                var effectiveTargetStates = transition.GetEffectiveTargetStates(_executionContext, _root);

                foreach (var state in effectiveTargetStates)
                {
                    AddAncestorStatesToEnter(state, ancestor, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }
            }
        }

        private void AddAncestorStatesToEnter(State state,
                                              State ancestor,
                                              Set<State> statesToEnter,
                                              Set<State> statesForDefaultEntry,
                                              Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            var ancestors = state.GetProperAncestors(ancestor);

            foreach (var anc in ancestors)
            {
                statesToEnter.Add(anc);

                if (anc.IsParallelState)
                {
                    var childStates = anc.GetChildStates();

                    foreach (var child in childStates)
                    {
                        if (! statesToEnter.Any(s => s.IsDescendent(child)))
                        {
                            AddDescendentStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }

        private void AddDescendentStatesToEnter(State state,
                                                Set<State> statesToEnter,
                                                Set<State> statesForDefaultEntry,
                                                Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            if (state.IsHistoryState)
            {
                if (_executionContext.TryGetHistoryValue(state.Id, out IEnumerable<State> states))
                {
                    foreach (var s in states)
                    {
                        AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in states)
                    {
                        AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else
                {
                    var targetStates = new List<State>();

                    ((HistoryState) state).VisitTransition(targetStates, defaultHistoryContent, _root);

                    foreach (var s in targetStates)
                    {
                        AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in targetStates)
                    {
                        AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
            }
            else
            {
                statesToEnter.Add(state);

                if (state.IsSequentialState)
                {
                    statesForDefaultEntry.Add(state);

                    var initialTransition = state.GetInitialStateTransition();

                    var targetStates = initialTransition.GetTargetStates(_root);

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
                    var childStates = state.GetChildStates();

                    foreach (var child in childStates)
                    {
                        if (! statesToEnter.Any(s => s.IsDescendent(child)))
                        {
                            AddDescendentStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }
    }
}
