using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.Model.States;
using StateChartsDotNet.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common;

namespace StateChartsDotNet
{
    public class Interpreter
    {
        public Interpreter()
        {
        }

        public async Task RunAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            await context.InitAsync();

            if (context.Root.Binding == Databinding.Early)
            {
                context.Root.InitDatamodel(context, true);
            }

            context.IsRunning = true;

            context.Root.ExecuteScript(context);

            await EnterStatesAsync(context,
                                   new List<Transition>(new[] { context.Root.GetInitialStateTransition() }));

            await DoMessageLoopAsync(context);
        }

        private async Task DoMessageLoopAsync(ExecutionContext context)
        {
            Debug.Assert(context != null);

            await context.LogInformationAsync("Start: event loop");

            while (context.IsRunning)
            {
                await context.LogInformationAsync("Start: event loop cycle");

                Set<Transition> enabledTransitions = null;

                var macrostepDone = false;

                while (context.IsRunning && !macrostepDone)
                {
                    enabledTransitions = SelectMessagelessTransitions(context);

                    if (enabledTransitions.IsEmpty())
                    {
                        var internalMessage = context.DequeueInternal();

                        if (internalMessage == null)
                        {
                            macrostepDone = true;
                        }
                        else
                        {
                            enabledTransitions = SelectTransitions(context, internalMessage);
                        }
                    }

                    if (!enabledTransitions.IsEmpty())
                    {
                        await MicrostepAsync(context, enabledTransitions);
                    }
                }

                if (!context.IsRunning)
                {
                    await context.LogInformationAsync("End: event loop cycle");
                    break;
                }

                foreach (var state in context.StatesToInvoke.Sort(State.Compare))
                {
                    await state.InvokeAsync(context);
                }

                context.StatesToInvoke.Clear();

                if (context.HasInternalMessages)
                {
                    await context.LogInformationAsync("End: event loop cycle");
                    continue;
                }

                var externalMessage = await context.DequeueExternalAsync();

                if (externalMessage.IsCancel)
                {
                    context.IsRunning = false;
                    
                    await context.LogInformationAsync("End: event loop cycle");
                    
                    continue;
                }

                foreach (var state in context.Configuration)
                {
                    await state.ProcessExternalMessageAsync(context, externalMessage);
                }

                enabledTransitions = SelectTransitions(context, externalMessage);

                if (!enabledTransitions.IsEmpty())
                {
                    await MicrostepAsync(context, enabledTransitions);
                }

                await context.LogInformationAsync("End: event loop cycle");
            }

            foreach (var state in context.Configuration.Sort(State.ReverseCompare))
            {
                await state.ExitAsync(context);

                if (state.IsFinalState)
                {
                    if (state.Parent.IsScxmlRoot)
                    {
                        ReturnDoneMessage(state);
                    }
                }
            }

            await context.LogInformationAsync("End: event loop");
        }

        private void ReturnDoneMessage(State state)
        {
            // The implementation of returnDoneMessage is platform-dependent, but if this session is the result of an <invoke> in another SCXML session, 
            //  returnDoneMessage will cause the event done.invoke.<id> to be placed in the external event queue of that session, where <id> is the id 
            //  generated in that session when the <invoke> was executed.
        }

        private async Task MicrostepAsync(ExecutionContext context, IEnumerable<Transition> enabledTransitions)
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

        private async Task ExitStatesAsync(ExecutionContext context, IEnumerable<Transition> enabledTransitions)
        {
            var exitSet = ComputeExitSet(context, enabledTransitions);

            Debug.Assert(exitSet != null);

            foreach (var state in exitSet)
            {
                Debug.Assert(state != null);

                context.StatesToInvoke.Remove(state);
            }

            foreach (var state in exitSet.Sort(State.ReverseCompare))
            {
                Debug.Assert(state != null);

                state.RecordHistory(context);

                await state.ExitAsync(context);
            }
        }

        private Set<State> ComputeExitSet(ExecutionContext context, IEnumerable<Transition> transitions)
        {
            Debug.Assert(context != null);
            Debug.Assert(transitions != null);

            var statesToExit = new Set<State>();

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

        private Set<Transition> SelectTransitions(ExecutionContext context, Message evt)
        {
            return SelectTransitions(context,
                                     transition => transition.MatchesMessage(evt) &&
                                                   transition.EvaluateCondition(context));
        }

        private Set<Transition> SelectMessagelessTransitions(ExecutionContext context)
        {
            return SelectTransitions(context,
                                     transition => !transition.HasMessage &&
                                                   transition.EvaluateCondition(context));
        }

        private Set<Transition> SelectTransitions(ExecutionContext context, Func<Transition, bool> predicate)
        {
            Debug.Assert(context != null);
            Debug.Assert(predicate != null);

            var enabledTransitions = new Set<Transition>();

            var atomicStates = context.Configuration.Sort(State.Compare).Where(s => s.IsAtomic);

            foreach (var state in atomicStates)
            {
                var all = new List<State>
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

        private Set<Transition> RemoveConflictingTransitions(ExecutionContext context,
                                                             IEnumerable<Transition> enabledTransitions)
        {
            Debug.Assert(context != null);
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

                    var exitSet1 = ComputeExitSet(context, new List<Transition> { transition1 });

                    Debug.Assert(exitSet1 != null);

                    var exitSet2 = ComputeExitSet(context, new List<Transition> { transition2 });

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

        private async Task EnterStatesAsync(ExecutionContext context, IEnumerable<Transition> enabledTransitions)
        {
            var statesToEnter = new Set<State>();

            var statesForDefaultEntry = new Set<State>();

            var defaultHistoryContent = new Dictionary<string, Set<ExecutableContent>>();

            await ComputeEntrySetAsync(context, enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

            foreach (var state in statesToEnter.Sort(State.Compare))
            {
                Debug.Assert(state != null);

                await state.EnterAsync(context, statesForDefaultEntry, defaultHistoryContent);
            }
        }

        private async Task ComputeEntrySetAsync(ExecutionContext context,
                                                IEnumerable<Transition> enabledTransitions,
                                                Set<State> statesToEnter,
                                                Set<State> statesForDefaultEntry,
                                                Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
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

        private async Task AddAncestorStatesToEnterAsync(ExecutionContext context,
                                                         State state,
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
                            await AddDescendentStatesToEnterAsync(context, child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }

        private async Task AddDescendentStatesToEnterAsync(ExecutionContext context,
                                                           State state,
                                                           Set<State> statesToEnter,
                                                           Set<State> statesForDefaultEntry,
                                                           Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            Debug.Assert(context != null);
            Debug.Assert(state != null);
            Debug.Assert(statesToEnter != null);
            Debug.Assert(statesForDefaultEntry != null);

            if (state.IsHistoryState)
            {
                if (context.TryGetHistoryValue(state.Id, out IEnumerable<State> resolvedStates))
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
                    var targetStates = new List<State>();

                    ((HistoryState) state).VisitTransition(targetStates, defaultHistoryContent, context.Root);

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

                if (state.IsSequentialState)
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
                else if (state.IsParallelState)
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
