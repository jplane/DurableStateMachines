using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using CoreEngine.Model;
using CoreEngine.Model.States;
using CoreEngine.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreEngine.Abstractions.Model;
using Nito.AsyncEx;

namespace CoreEngine
{
    public class Interpreter
    {
        private readonly AsyncLazy<RootState> _root;
        private readonly ExecutionContext _executionContext;

        public Interpreter(IModelMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _root = new AsyncLazy<RootState>(async () =>
            {
                return new RootState(await metadata.GetRootState());
            });

            _executionContext = new ExecutionContext();

            _executionContext.SetDataValue("_sessionid", Guid.NewGuid().ToString("D"));
        }

        public ExecutionContext Context => _executionContext;

        public async Task Run()
        {
            _executionContext.SetDataValue("_name", (await _root).Name);

            if ((await _root).Binding == Databinding.Early)
            {
                await (await _root).InitDatamodel(_executionContext, true);
            }

            _executionContext.IsRunning = true;

            await (await _root).ExecuteScript(_executionContext);

            await EnterStates(new List<Transition>(new []{ await (await _root).GetInitialStateTransition() }));

            await DoEventLoop();
        }

        private async Task DoEventLoop()
        {
            _executionContext.LogInformation("Start: event loop");

            while (_executionContext.IsRunning)
            {
                _executionContext.LogInformation("Start: event loop cycle");

                Set<Transition> enabledTransitions = null;

                var macrostepDone = false;

                while (_executionContext.IsRunning && ! macrostepDone)
                {
                    enabledTransitions = await SelectEventlessTransitions();

                    if (enabledTransitions.IsEmpty())
                    {
                        var internalEvent = _executionContext.DequeueInternal();

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

                if (! _executionContext.IsRunning)
                {
                    _executionContext.LogInformation("End: event loop cycle");
                    break;
                }

                foreach (var state in _executionContext.StatesToInvoke.Sort(State.Compare))
                {
                    await state.Invoke(_executionContext, (await _root));
                }

                _executionContext.StatesToInvoke.Clear();

                if (_executionContext.HasInternalEvents)
                {
                    _executionContext.LogInformation("End: event loop cycle");
                    continue;
                }

                var externalEvent = await _executionContext.DequeueExternal();

                if (externalEvent.IsCancel)
                {
                    _executionContext.IsRunning = false;
                    _executionContext.LogInformation("End: event loop cycle");
                    continue;
                }

                foreach (var state in _executionContext.Configuration)
                {
                    await state.ProcessExternalEvent(_executionContext, externalEvent);
                }

                enabledTransitions = await SelectTransitions(externalEvent);

                if (! enabledTransitions.IsEmpty())
                {
                    await Microstep(enabledTransitions);
                }

                _executionContext.LogInformation("End: event loop cycle");
            }

            foreach (var state in _executionContext.Configuration.Sort(State.ReverseCompare))
            {
                await state.Exit(_executionContext);

                if (state.IsFinalState)
                {
                    if (state.Parent.IsScxmlRoot)
                    {
                        ReturnDoneEvent(state);
                    }
                }
            }

            _executionContext.LogInformation("End: event loop");
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
                await transition.ExecuteContent(_executionContext);
            }

            await EnterStates(enabledTransitions);
        }

        private async Task ExitStates(IEnumerable<Transition> enabledTransitions)
        {
            var exitSet = await ComputeExitSet(enabledTransitions);

            Debug.Assert(exitSet != null);

            foreach (var state in exitSet)
            {
                _executionContext.StatesToInvoke.Remove(state);
            }

            foreach (var state in exitSet.Sort(State.ReverseCompare))
            {
                await state.RecordHistory(_executionContext);

                await state.Exit(_executionContext);
            }
        }

        private async Task<Set<State>> ComputeExitSet(IEnumerable<Transition> transitions)
        {
            Debug.Assert(transitions != null);

            var statesToExit = new Set<State>();

            foreach (var transition in transitions)
            {
                if (transition.HasTargets)
                {
                    var domain = await transition.GetTransitionDomain(_executionContext, (await _root));

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

        private Task<Set<Transition>> SelectTransitions(Event evt)
        {
            return SelectTransitions(async transition => transition.MatchesEvent(evt) &&
                                                         await transition.EvaluateCondition(_executionContext));
        }

        private Task<Set<Transition>> SelectEventlessTransitions()
        {
            return SelectTransitions(async transition => !transition.HasEvent &&
                                                         await transition.EvaluateCondition(_executionContext));
        }

        private async Task<Set<Transition>> SelectTransitions(Func<Transition, Task<bool>> predicate)
        {
            Debug.Assert(predicate != null);

            var enabledTransitions = new Set<Transition>();

            var atomicStates = _executionContext.Configuration
                                                .Sort(State.Compare)
                                                .Where(s => s.IsAtomic);

            foreach (var state in atomicStates)
            {
                var all = new List<State>
                {
                    state
                };

                foreach (var anc in state.GetProperAncestors((await _root)))
                {
                    all.Add(anc);
                }

                foreach (var s in all)
                {
                    foreach (var transition in await s.GetTransitions())
                    {
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

            return enabledTransitions;
        }

        private async Task<Set<Transition>> RemoveConflictingTransitions(IEnumerable<Transition> enabledTransitions)
        {
            Debug.Assert(enabledTransitions != null);

            var filteredTransitions = new Set<Transition>();

            foreach (var transition1 in enabledTransitions)
            {
                var t1Preempted = false;

                var transitionsToRemove = new Set<Transition>();

                foreach (var transition2 in filteredTransitions)
                {
                    var exitSet1 = await ComputeExitSet(new List<Transition> { transition1 });

                    var exitSet2 = await ComputeExitSet(new List<Transition> { transition2 });

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

        private async Task EnterStates(IEnumerable<Transition> enabledTransitions)
        {
            var statesToEnter = new Set<State>();

            var statesForDefaultEntry = new Set<State>();

            var defaultHistoryContent = new Dictionary<string, Set<ExecutableContent>>();

            await ComputeEntrySet(enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

            foreach (var state in statesToEnter.Sort(State.Compare))
            {
                await state.Enter(_executionContext, (await _root), statesForDefaultEntry, defaultHistoryContent);
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

                foreach (var state in await transition.GetTargetStates((await _root)))
                {
                    await AddDescendentStatesToEnter(state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }

                var ancestor = await transition.GetTransitionDomain(_executionContext, (await _root));

                var effectiveTargetStates = await transition.GetEffectiveTargetStates(_executionContext, (await _root));

                foreach (var state in effectiveTargetStates)
                {
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

            foreach (var anc in ancestors)
            {
                statesToEnter.Add(anc);

                if (anc.IsParallelState)
                {
                    var childStates = await anc.GetChildStates();

                    foreach (var child in childStates)
                    {
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
                if (_executionContext.TryGetHistoryValue(state.Id, out IEnumerable<State> states))
                {
                    foreach (var s in states)
                    {
                        await AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in states)
                    {
                        await AddAncestorStatesToEnter(s, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else
                {
                    var targetStates = new List<State>();

                    await ((HistoryState) state).VisitTransition(targetStates, defaultHistoryContent, (await _root));

                    foreach (var s in targetStates)
                    {
                        await AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in targetStates)
                    {
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

                    var targetStates = await initialTransition.GetTargetStates((await _root));

                    foreach (var s in targetStates)
                    {
                        await AddDescendentStatesToEnter(s, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var s in targetStates)
                    {
                        await AddAncestorStatesToEnter(s, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else if (state.IsParallelState)
                {
                    var childStates = await state.GetChildStates();

                    foreach (var child in childStates)
                    {
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
