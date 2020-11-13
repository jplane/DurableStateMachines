using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.CoreEngine.Model.States;
using StateChartsDotNet.CoreEngine.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using Nito.AsyncEx;

namespace StateChartsDotNet.CoreEngine
{
    public class Interpreter
    {
        private readonly AsyncLazy<RootState> _root;

        public Interpreter(IModelMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _root = new AsyncLazy<RootState>(async () =>
            {
                return new RootState(await metadata.GetRootState());
            });

            Context = new ExecutionContext();

            Context.SetDataValue("_sessionid", Guid.NewGuid().ToString("D"));
        }

        public ExecutionContext Context { get; }

        public async Task Run()
        {
            Context.SetDataValue("_name", (await _root).Name);

            if ((await _root).Binding == Databinding.Early)
            {
                await (await _root).InitDatamodel(Context, true);
            }

            Context.IsRunning = true;

            await (await _root).ExecuteScript(Context);

            await EnterStates(new List<Transition>(new []{ await (await _root).GetInitialStateTransition() }));

            await DoEventLoop();
        }

        private async Task DoEventLoop()
        {
            Context.LogInformation("Start: event loop");

            while (Context.IsRunning)
            {
                Context.LogInformation("Start: event loop cycle");

                Set<Transition> enabledTransitions = null;

                var macrostepDone = false;

                while (Context.IsRunning && ! macrostepDone)
                {
                    enabledTransitions = await SelectEventlessTransitions();

                    if (enabledTransitions.IsEmpty())
                    {
                        var internalEvent = Context.DequeueInternal();

                        if (internalEvent == null)
                        {
                            macrostepDone = true;
                        }
                        else
                        {
                            enabledTransitions = await SelectTransitions(internalEvent);
                        }
                    }

                    if (! enabledTransitions.IsEmpty())
                    {
                        await Microstep(enabledTransitions);
                    }
                }

                if (! Context.IsRunning)
                {
                    Context.LogInformation("End: event loop cycle");
                    break;
                }

                foreach (var state in Context.StatesToInvoke.Sort(State.Compare))
                {
                    await state.Invoke(Context, await _root);
                }

                Context.StatesToInvoke.Clear();

                if (Context.HasInternalEvents)
                {
                    Context.LogInformation("End: event loop cycle");
                    continue;
                }

                var externalEvent = await Context.DequeueExternal();

                if (externalEvent.IsCancel)
                {
                    Context.IsRunning = false;
                    Context.LogInformation("End: event loop cycle");
                    continue;
                }

                foreach (var state in Context.Configuration)
                {
                    await state.ProcessExternalEvent(Context, externalEvent);
                }

                enabledTransitions = await SelectTransitions(externalEvent);

                if (! enabledTransitions.IsEmpty())
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
                        ReturnDoneEvent(state);
                    }
                }
            }

            Context.LogInformation("End: event loop");
        }

        private void ReturnDoneEvent(State state)
        {
            // The implementation of returnDoneEvent is platform-dependent, but if this session is the result of an <invoke> in another SCXML session, 
            //  returnDoneEvent will cause the event done.invoke.<id> to be placed in the external event queue of that session, where <id> is the id 
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
            var exitSet = await ComputeExitSet(enabledTransitions);

            Debug.Assert(exitSet != null);

            foreach (var state in exitSet)
            {
                Debug.Assert(state != null);

                Context.StatesToInvoke.Remove(state);
            }

            foreach (var state in exitSet.Sort(State.ReverseCompare))
            {
                Debug.Assert(state != null);

                await state.RecordHistory(Context);

                await state.Exit(Context);
            }
        }

        private async Task<Set<State>> ComputeExitSet(IEnumerable<Transition> transitions)
        {
            Debug.Assert(transitions != null);

            var statesToExit = new Set<State>();

            foreach (var transition in transitions)
            {
                Debug.Assert(transition != null);

                if (transition.HasTargets)
                {
                    var domain = await transition.GetTransitionDomain(Context, await _root);

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

        private Task<Set<Transition>> SelectTransitions(Event evt)
        {
            return SelectTransitions(async transition => transition.MatchesEvent(evt) &&
                                                         await transition.EvaluateCondition(Context));
        }

        private Task<Set<Transition>> SelectEventlessTransitions()
        {
            return SelectTransitions(async transition => !transition.HasEvent &&
                                                         await transition.EvaluateCondition(Context));
        }

        private async Task<Set<Transition>> SelectTransitions(Func<Transition, Task<bool>> predicate)
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

                foreach (var anc in state.GetProperAncestors(await _root))
                {
                    all.Add(anc);
                }

                foreach (var s in all)
                {
                    Debug.Assert(s != null);

                    foreach (var transition in await s.GetTransitions())
                    {
                        Debug.Assert(transition != null);

                        if (await predicate(transition))
                        {
                            enabledTransitions.Add(transition);
                            goto nextAtomicState;
                        }
                    }
                }

                nextAtomicState:
                    continue;
            }

            enabledTransitions = await RemoveConflictingTransitions(enabledTransitions);

            Debug.Assert(enabledTransitions != null);

            return enabledTransitions;
        }

        private async Task<Set<Transition>> RemoveConflictingTransitions(IEnumerable<Transition> enabledTransitions)
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

                    var exitSet1 = await ComputeExitSet(new List<Transition> { transition1 });

                    Debug.Assert(exitSet1 != null);

                    var exitSet2 = await ComputeExitSet(new List<Transition> { transition2 });

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

                await state.Enter(Context, await _root, statesForDefaultEntry, defaultHistoryContent);
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

                foreach (var state in await transition.GetTargetStates(await _root))
                {
                    Debug.Assert(state != null);

                    await AddDescendentStatesToEnter(state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }

                var ancestor = await transition.GetTransitionDomain(Context, await _root);

                var effectiveTargetStates = await transition.GetEffectiveTargetStates(Context, await _root);

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
                    var childStates = await anc.GetChildStates();

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

                    await ((HistoryState) state).VisitTransition(targetStates, defaultHistoryContent, await _root);

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

                    var initialTransition = await state.GetInitialStateTransition();

                    Debug.Assert(initialTransition != null);

                    var targetStates = await initialTransition.GetTargetStates(await _root);

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
                    var childStates = await state.GetChildStates();

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
