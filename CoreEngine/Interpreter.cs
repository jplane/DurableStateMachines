using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.CoreEngine.Model.States;
using StateChartsDotNet.CoreEngine.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;

namespace StateChartsDotNet.CoreEngine
{
    public class Interpreter
    {
        private readonly Lazy<RootState> _root;

        public Interpreter(IModelMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _root = new Lazy<RootState>(() =>
            {
                return new RootState(metadata.GetRootState());
            });

            Context = new ExecutionContext();

            Context.SetDataValue("_sessionid", Guid.NewGuid().ToString("D"));
        }

        public ExecutionContext Context { get; }

        public async Task Run()
        {
            Context.SetDataValue("_name", _root.Value.Name);

            if (_root.Value.Binding == Databinding.Early)
            {
                _root.Value.InitDatamodel(Context, true);
            }

            Context.IsRunning = true;

            _root.Value.ExecuteScript(Context);

            await EnterStates(new List<Transition>(new[] { _root.Value.GetInitialStateTransition() }));

            await DoMessageLoop();
        }

        private async Task DoMessageLoop()
        {
            Context.LogInformation("Start: event loop");

            while (Context.IsRunning)
            {
                Context.LogInformation("Start: event loop cycle");

                Set<Transition> enabledTransitions = null;

                var macrostepDone = false;

                while (Context.IsRunning && !macrostepDone)
                {
                    enabledTransitions = SelectMessagelessTransitions();

                    if (enabledTransitions.IsEmpty())
                    {
                        var internalMessage = Context.DequeueInternal();

                        if (internalMessage == null)
                        {
                            macrostepDone = true;
                        }
                        else
                        {
                            enabledTransitions = SelectTransitions(internalMessage);
                        }
                    }

                    if (!enabledTransitions.IsEmpty())
                    {
                        await Microstep(enabledTransitions);
                    }
                }

                if (!Context.IsRunning)
                {
                    Context.LogInformation("End: event loop cycle");
                    break;
                }

                foreach (var state in Context.StatesToInvoke.Sort(State.Compare))
                {
                    await state.Invoke(Context, _root.Value);
                }

                Context.StatesToInvoke.Clear();

                if (Context.HasInternalMessages)
                {
                    Context.LogInformation("End: event loop cycle");
                    continue;
                }

                var externalMessage = await Context.DequeueExternal();

                if (externalMessage.IsCancel)
                {
                    Context.IsRunning = false;
                    Context.LogInformation("End: event loop cycle");
                    continue;
                }

                foreach (var state in Context.Configuration)
                {
                    await state.ProcessExternalMessage(Context, externalMessage);
                }

                enabledTransitions = await SelectTransitions(externalMessage);

                if (!enabledTransitions.IsEmpty())
                {
                    await Microstep(enabledTransitions);
                }

                Context.LogInformation("End: event loop cycle");
            }

            foreach (var state in Context.Configuration.Sort(State.ReverseCompare))
            {
                await state.Exit(Context);

                if (state.IsFinalState)
                {
                    if (state.Parent.IsScxmlRoot)
                    {
                        ReturnDoneMessage(state);
                    }
                }
            }

            Context.LogInformation("End: event loop");
        }

        private void ReturnDoneMessage(State state)
        {
            // The implementation of returnDoneMessage is platform-dependent, but if this session is the result of an <invoke> in another SCXML session, 
            //  returnDoneMessage will cause the event done.invoke.<id> to be placed in the external event queue of that session, where <id> is the id 
            //  generated in that session when the <invoke> was executed.
        }

        private async Task Microstep(IEnumerable<Transition> enabledTransitions)
        {
            Debug.Assert(enabledTransitions != null);

            await ExitStates(enabledTransitions);
            
            foreach (var transition in enabledTransitions)
            {
                Debug.Assert(transition != null);

                await transition.ExecuteContent(Context);
            }

            await EnterStates(enabledTransitions);
        }

        private async Task ExitStates(IEnumerable<Transition> enabledTransitions)
        {
            var exitSet = ComputeExitSet(enabledTransitions);

            Debug.Assert(exitSet != null);

            foreach (var state in exitSet)
            {
                Debug.Assert(state != null);

                Context.StatesToInvoke.Remove(state);
            }

            foreach (var state in exitSet.Sort(State.ReverseCompare))
            {
                Debug.Assert(state != null);

                state.RecordHistory(Context);

                await state.Exit(Context);
            }
        }

        private Set<State> ComputeExitSet(IEnumerable<Transition> transitions)
        {
            Debug.Assert(transitions != null);

            var statesToExit = new Set<State>();

            foreach (var transition in transitions)
            {
                Debug.Assert(transition != null);

                if (transition.HasTargets)
                {
                    var domain = transition.GetTransitionDomain(Context, _root.Value);

                    Debug.Assert(domain != null);

                    foreach (var state in Context.Configuration)
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

        private Set<Transition> SelectTransitions(Message evt)
        {
            return SelectTransitions(transition => transition.MatchesMessage(evt) &&
                                                   transition.EvaluateCondition(Context));
        }

        private Set<Transition> SelectMessagelessTransitions()
        {
            return SelectTransitions(transition => !transition.HasMessage &&
                                                   transition.EvaluateCondition(Context));
        }

        private Set<Transition> SelectTransitions(Func<Transition, bool> predicate)
        {
            Debug.Assert(predicate != null);

            var enabledTransitions = new Set<Transition>();

            var atomicStates = Context.Configuration.Sort(State.Compare).Where(s => s.IsAtomic);

            foreach (var state in atomicStates)
            {
                var all = new List<State>
                {
                    state
                };

                foreach (var anc in state.GetProperAncestors(_root.Value))
                {
                    all.Add(anc);
                }

                foreach (var s in all)
                {
                    Debug.Assert(s != null);

                    foreach (var transition in s.GetTransitions())
                    {
                        Debug.Assert(transition != null);

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

            Debug.Assert(enabledTransitions != null);

            return enabledTransitions;
        }

        private Set<Transition> RemoveConflictingTransitions(IEnumerable<Transition> enabledTransitions)
        {
            Debug.Assert(enabledTransitions != null);

            var filteredTransitions = new Set<Transition>();

            foreach (var transition1 in enabledTransitions)
            {
                Debug.Assert(transition1 != null);

                var t1Preempted = false;

                var transitionsToRemove = new Set<Transition>();

                foreach (var transition2 in filteredTransitions)
                {
                    Debug.Assert(transition2 != null);

                    var exitSet1 = ComputeExitSet(new List<Transition> { transition1 });

                    Debug.Assert(exitSet1 != null);

                    var exitSet2 = ComputeExitSet(new List<Transition> { transition2 });

                    Debug.Assert(exitSet2 != null);

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
                        Debug.Assert(transition3 != null);

                        filteredTransitions.Remove(transition3);
                    }

                    filteredTransitions.Add(transition1);
                }
            }

            return filteredTransitions;
        }

        private async Task EnterStates(IEnumerable<Transition> enabledTransitions)
        {
            var statesToEnter = new Set<State>();

            var statesForDefaultEntry = new Set<State>();

            var defaultHistoryContent = new Dictionary<string, Set<ExecutableContent>>();

            await ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

            foreach (var state in statesToEnter.Sort(State.Compare))
            {
                Debug.Assert(state != null);

                await state.Enter(Context, _root.Value, statesForDefaultEntry, defaultHistoryContent);
            }
        }

        private async Task ComputeEntrySet(IEnumerable<Transition> enabledTransitions,
                                           Set<State> statesToEnter,
                                           Set<State> statesForDefaultEntry,
                                           Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            Debug.Assert(enabledTransitions != null);

            foreach (var transition in enabledTransitions)
            {
                Debug.Assert(transition != null);

                foreach (var state in transition.GetTargetStates(_root.Value))
                {
                    Debug.Assert(state != null);

                    await AddDescendentStatesToEnter(state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }

                var ancestor = transition.GetTransitionDomain(Context, _root.Value);

                var effectiveTargetStates = transition.GetEffectiveTargetStates(Context, _root.Value);

                Debug.Assert(effectiveTargetStates != null);

                foreach (var state in effectiveTargetStates)
                {
                    Debug.Assert(state != null);

                    await AddAncestorStatesToEnter(state, ancestor, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }
            }
        }

        private async Task AddAncestorStatesToEnter(State state,
                                                    State ancestor,
                                                    Set<State> statesToEnter,
                                                    Set<State> statesForDefaultEntry,
                                                    Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            Debug.Assert(state != null);
            Debug.Assert(statesToEnter != null);

            var ancestors = state.GetProperAncestors(ancestor);

            Debug.Assert(ancestors != null);

            foreach (var anc in ancestors)
            {
                Debug.Assert(anc != null);

                statesToEnter.Add(anc);

                if (anc.IsParallelState)
                {
                    var childStates = anc.GetChildStates();

                    Debug.Assert(childStates != null);

                    foreach (var child in childStates)
                    {
                        Debug.Assert(child != null);

                        if (! statesToEnter.Any(s => s.IsDescendent(child)))
                        {
                            await AddDescendentStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }

        private async Task AddDescendentStatesToEnter(State state,
                                                      Set<State> statesToEnter,
                                                      Set<State> statesForDefaultEntry,
                                                      Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            Debug.Assert(state != null);
            Debug.Assert(statesToEnter != null);
            Debug.Assert(statesForDefaultEntry != null);

            if (state.IsHistoryState)
            {
                if (Context.TryGetHistoryValue(state.Id, out IEnumerable<State> states))
                {
                    foreach (var s in states)
                    {
                        Debug.Assert(s != null);

                        await AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in states)
                    {
                        Debug.Assert(s != null);

                        await AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else
                {
                    var targetStates = new List<State>();

                    ((HistoryState) state).VisitTransition(targetStates, defaultHistoryContent, _root.Value);

                    foreach (var s in targetStates)
                    {
                        Debug.Assert(s != null);

                        await AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in targetStates)
                    {
                        Debug.Assert(s != null);

                        await AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
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

                    Debug.Assert(initialTransition != null);

                    var targetStates = initialTransition.GetTargetStates(_root.Value);

                    Debug.Assert(targetStates != null);

                    foreach (var s in targetStates)
                    {
                        Debug.Assert(s != null);

                        await AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in targetStates)
                    {
                        Debug.Assert(s != null);

                        await AddAncestorStatesToEnter(s, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else if (state.IsParallelState)
                {
                    var childStates = state.GetChildStates();

                    Debug.Assert(childStates != null);

                    foreach (var child in childStates)
                    {
                        Debug.Assert(child != null);

                        if (! statesToEnter.Any(s => s.IsDescendent(child)))
                        {
                            await AddDescendentStatesToEnter(child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }
    }
}
