using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.Model.States;
using StateChartsDotNet.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;

namespace StateChartsDotNet
{
    internal class Interpreter<TData>
    {
        public Interpreter()
        {
        }

        public async Task RunAsync(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: event loop");

            await EnterStatechartAsync(context);

            while (context.IsRunning)
            {
                await context.LogInformationAsync("Start: event loop cycle");

                await MacrostepAsync(context);

                if (context.IsRunning)
                {
                    await ProcessStateChartInvokesAsync(context);

                    if (!context.HasInternalMessages)
                    {
                        await ProcessExternalMessageAsync(context);
                    }
                }

                await context.LogInformationAsync("End: event loop cycle");
            }

            await ExitStatechartAsync(context);

            await context.LogInformationAsync("End: event loop");

            context.CheckErrorPropagation();
        }

        private async Task EnterStatechartAsync(ExecutionContextBase<TData> context)
        {
            Debug.Assert(context != null);

            await context.InitAsync();

            await EnterStatesAsync(context,
                                   new List<Transition<TData>>(new[] { context.Root.GetInitialStateTransition() }));
        }

        private static async Task ExitStatechartAsync(ExecutionContextBase<TData> context)
        {
            Debug.Assert(context != null);

            foreach (var state in context.Configuration.Sort(State<TData>.ReverseCompare))
            {
                await state.ExitAsync(context);
            }

            await context.ExitAsync();
        }

        private static async Task ProcessStateChartInvokesAsync(ExecutionContextBase<TData> context)
        {
            Debug.Assert(context != null);

            foreach (var state in context.StatesToInvoke.Sort(State<TData>.Compare))
            {
                await state.InvokeAsync(context);
            }

            context.StatesToInvoke.Clear();
        }

        private async Task ProcessExternalMessageAsync(ExecutionContextBase<TData> context)
        {
            Debug.Assert(context != null);

            var externalMessage = await context.DequeueExternalAsync();

            if (context.IsRunning)
            {
                var enabledTransitions = SelectTransitions(context, externalMessage);

                if (!enabledTransitions.IsEmpty())
                {
                    await MicrostepAsync(context, enabledTransitions);
                }
            }
        }

        private async Task MacrostepAsync(ExecutionContextBase<TData> context)
        {
            Debug.Assert(context != null);

            var macrostepDone = false;

            while (context.IsRunning && !macrostepDone)
            {
                var enabledTransitions = SelectMessagelessTransitions(context);

                Debug.Assert(enabledTransitions != null);

                if (enabledTransitions.IsEmpty())
                {
                    var internalMessage = context.DequeueInternal();

                    if (context.IsRunning)
                    {
                        if (internalMessage == null)
                        {
                            macrostepDone = true;
                        }
                        else
                        {
                            enabledTransitions = SelectTransitions(context, internalMessage);
                        }
                    }
                }

                if (!enabledTransitions.IsEmpty())
                {
                    await MicrostepAsync(context, enabledTransitions);
                }
            }
        }

        private async Task MicrostepAsync(ExecutionContextBase<TData> context, IEnumerable<Transition<TData>> enabledTransitions)
        {
            Debug.Assert(enabledTransitions != null);

            await ExitStatesAsync(context, enabledTransitions);
            
            foreach (var transition in enabledTransitions)
            {
                Debug.Assert(transition != null);

                await transition.ExecuteContentAsync(context);
            }

            await EnterStatesAsync(context, enabledTransitions);
        }

        private async Task ExitStatesAsync(ExecutionContextBase<TData> context, IEnumerable<Transition<TData>> enabledTransitions)
        {
            var exitSet = ComputeExitSet(context, enabledTransitions);

            Debug.Assert(exitSet != null);

            foreach (var state in exitSet)
            {
                Debug.Assert(state != null);

                context.StatesToInvoke.Remove(state);
            }

            foreach (var state in exitSet.Sort(State<TData>.ReverseCompare))
            {
                Debug.Assert(state != null);

                state.RecordHistory(context);

                await state.ExitAsync(context);
            }
        }

        private Set<State<TData>> ComputeExitSet(ExecutionContextBase<TData> context, IEnumerable<Transition<TData>> transitions)
        {
            Debug.Assert(context != null);
            Debug.Assert(transitions != null);

            var statesToExit = new Set<State<TData>>();

            foreach (var transition in transitions)
            {
                Debug.Assert(transition != null);

                if (transition.HasTargets)
                {
                    var domain = transition.GetTransitionDomain(context);

                    Debug.Assert(domain != null);

                    foreach (var state in context.Configuration)
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

        private Set<Transition<TData>> SelectTransitions(ExecutionContextBase<TData> context, Message evt)
        {
            return SelectTransitions(context,
                                     transition => transition.MatchesMessage(evt) &&
                                                   transition.EvaluateCondition(context));
        }

        private Set<Transition<TData>> SelectMessagelessTransitions(ExecutionContextBase<TData> context)
        {
            return SelectTransitions(context,
                                     transition => !transition.HasMessage &&
                                                   transition.EvaluateCondition(context));
        }

        private Set<Transition<TData>> SelectTransitions(ExecutionContextBase<TData> context, Func<Transition<TData>, bool> predicate)
        {
            Debug.Assert(context != null);
            Debug.Assert(predicate != null);

            var enabledTransitions = new Set<Transition<TData>>();

            var atomicStates = context.Configuration.Sort(State<TData>.Compare).Where(s => s.Type == StateType.Atomic ||
                                                                                    s.Type == StateType.Final);

            foreach (var state in atomicStates)
            {
                var all = new List<State<TData>>
                {
                    state
                };

                foreach (var anc in state.GetProperAncestors())
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

            enabledTransitions = RemoveConflictingTransitions(context, enabledTransitions);

            Debug.Assert(enabledTransitions != null);

            return enabledTransitions;
        }

        private Set<Transition<TData>> RemoveConflictingTransitions(ExecutionContextBase<TData> context,
                                                             IEnumerable<Transition<TData>> enabledTransitions)
        {
            Debug.Assert(context != null);
            Debug.Assert(enabledTransitions != null);

            var filteredTransitions = new Set<Transition<TData>>();

            foreach (var transition1 in enabledTransitions)
            {
                Debug.Assert(transition1 != null);

                var t1Preempted = false;

                var transitionsToRemove = new Set<Transition<TData>>();

                foreach (var transition2 in filteredTransitions)
                {
                    Debug.Assert(transition2 != null);

                    var exitSet1 = ComputeExitSet(context, new List<Transition<TData>> { transition1 });

                    Debug.Assert(exitSet1 != null);

                    var exitSet2 = ComputeExitSet(context, new List<Transition<TData>> { transition2 });

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

        private async Task EnterStatesAsync(ExecutionContextBase<TData> context, IEnumerable<Transition<TData>> enabledTransitions)
        {
            var statesToEnter = new Set<State<TData>>();

            var statesForDefaultEntry = new Set<State<TData>>();

            var defaultHistoryContent = new Dictionary<string, Set<ExecutableContent<TData>>>();

            await ComputeEntrySetAsync(context, enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

            foreach (var state in statesToEnter.Sort(State<TData>.Compare))
            {
                Debug.Assert(state != null);

                await state.EnterAsync(context, statesForDefaultEntry, defaultHistoryContent);
            }
        }

        private async Task ComputeEntrySetAsync(ExecutionContextBase<TData> context,
                                                IEnumerable<Transition<TData>> enabledTransitions,
                                                Set<State<TData>> statesToEnter,
                                                Set<State<TData>> statesForDefaultEntry,
                                                Dictionary<string, Set<ExecutableContent<TData>>> defaultHistoryContent)
        {
            Debug.Assert(context != null);
            Debug.Assert(enabledTransitions != null);

            foreach (var transition in enabledTransitions)
            {
                Debug.Assert(transition != null);

                foreach (var state in transition.GetTargetStates(context.Root))
                {
                    Debug.Assert(state != null);

                    await AddDescendentStatesToEnterAsync(context, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }

                var ancestor = transition.GetTransitionDomain(context);

                var effectiveTargetStates = transition.GetEffectiveTargetStates(context);

                Debug.Assert(effectiveTargetStates != null);

                foreach (var state in effectiveTargetStates)
                {
                    Debug.Assert(state != null);

                    await AddAncestorStatesToEnterAsync(context, state, ancestor, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }
            }
        }

        private async Task AddAncestorStatesToEnterAsync(ExecutionContextBase<TData> context,
                                                         State<TData> state,
                                                         State<TData> ancestor,
                                                         Set<State<TData>> statesToEnter,
                                                         Set<State<TData>> statesForDefaultEntry,
                                                         Dictionary<string, Set<ExecutableContent<TData>>> defaultHistoryContent)
        {
            Debug.Assert(state != null);
            Debug.Assert(statesToEnter != null);

            var ancestors = state.GetProperAncestors(ancestor);

            Debug.Assert(ancestors != null);

            foreach (var anc in ancestors)
            {
                Debug.Assert(anc != null);

                statesToEnter.Add(anc);

                if (anc.Type == StateType.Parallel)
                {
                    var childStates = anc.GetChildStates();

                    Debug.Assert(childStates != null);

                    foreach (var child in childStates)
                    {
                        Debug.Assert(child != null);

                        if (! statesToEnter.Any(s => s.IsDescendent(child)))
                        {
                            await AddDescendentStatesToEnterAsync(context, child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }

        private async Task AddDescendentStatesToEnterAsync(ExecutionContextBase<TData> context,
                                                           State<TData> state,
                                                           Set<State<TData>> statesToEnter,
                                                           Set<State<TData>> statesForDefaultEntry,
                                                           Dictionary<string, Set<ExecutableContent<TData>>> defaultHistoryContent)
        {
            Debug.Assert(context != null);
            Debug.Assert(state != null);
            Debug.Assert(statesToEnter != null);
            Debug.Assert(statesForDefaultEntry != null);

            if (state.Type == StateType.History)
            {
                if (context.TryGetHistoryValue(state.Id, out IEnumerable<State<TData>> resolvedStates))
                {
                    foreach (var resolved in resolvedStates)
                    {
                        Debug.Assert(resolved != null);

                        await AddDescendentStatesToEnterAsync(context, resolved, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var resolved in resolvedStates)
                    {
                        Debug.Assert(resolved != null);

                        await AddAncestorStatesToEnterAsync(context, resolved, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else
                {
                    var targetStates = new List<State<TData>>();

                    ((HistoryState<TData>) state).VisitTransition(targetStates, defaultHistoryContent, context.Root);

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddDescendentStatesToEnterAsync(context, target, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddAncestorStatesToEnterAsync(context, target, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
            }
            else
            {
                statesToEnter.Add(state);

                if (state.Type == StateType.Compound)
                {
                    statesForDefaultEntry.Add(state);

                    var initialTransition = state.GetInitialStateTransition();

                    Debug.Assert(initialTransition != null);

                    var targetStates = initialTransition.GetTargetStates(context.Root);

                    Debug.Assert(targetStates != null);

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddDescendentStatesToEnterAsync(context, target, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddAncestorStatesToEnterAsync(context, target, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else if (state.Type == StateType.Parallel)
                {
                    var childStates = state.GetChildStates();

                    Debug.Assert(childStates != null);

                    foreach (var child in childStates)
                    {
                        Debug.Assert(child != null);

                        if (! statesToEnter.Any(s => s.IsDescendent(child)))
                        {
                            await AddDescendentStatesToEnterAsync(context, child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }
    }
}
