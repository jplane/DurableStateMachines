using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.CoreEngine.Model.States;
using StateChartsDotNet.CoreEngine.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions;

namespace StateChartsDotNet.CoreEngine
{
    public class Interpreter
    {
        public Interpreter()
        {
        }

        public async Task Run(IModelMetadata metadata, ExecutionContext context)
        {
            metadata.CheckArgNull(nameof(metadata));
            context.CheckArgNull(nameof(context));

            var root = new RootState(metadata.GetRootState());

            await context.Init(root);

            if (root.Binding == Databinding.Early)
            {
                root.InitDatamodel(context, true);
            }

            context.IsRunning = true;

            root.ExecuteScript(context);

            await EnterStates(root,
                              context,
                              new List<Transition>(new[] { root.GetInitialStateTransition() }));

            await DoMessageLoop(root, context);
        }

        private async Task DoMessageLoop(RootState root, ExecutionContext context)
        {
            Debug.Assert(root != null);
            Debug.Assert(context != null);

            context.LogInformation("Start: event loop");

            while (context.IsRunning)
            {
                context.LogInformation("Start: event loop cycle");

                Set<Transition> enabledTransitions = null;

                var macrostepDone = false;

                while (context.IsRunning && !macrostepDone)
                {
                    enabledTransitions = SelectMessagelessTransitions(root, context);

                    if (enabledTransitions.IsEmpty())
                    {
                        var internalMessage = context.DequeueInternal();

                        if (internalMessage == null)
                        {
                            macrostepDone = true;
                        }
                        else
                        {
                            enabledTransitions = SelectTransitions(root, context, internalMessage);
                        }
                    }

                    if (!enabledTransitions.IsEmpty())
                    {
                        await Microstep(root, context, enabledTransitions);
                    }
                }

                if (!context.IsRunning)
                {
                    context.LogInformation("End: event loop cycle");
                    break;
                }

                foreach (var state in context.StatesToInvoke.Sort(State.Compare))
                {
                    await state.Invoke(context, root);
                }

                context.StatesToInvoke.Clear();

                if (context.HasInternalMessages)
                {
                    context.LogInformation("End: event loop cycle");
                    continue;
                }

                var externalMessage = await context.DequeueExternal();

                if (externalMessage.IsCancel)
                {
                    context.IsRunning = false;
                    context.LogInformation("End: event loop cycle");
                    continue;
                }

                foreach (var state in context.Configuration)
                {
                    await state.ProcessExternalMessage(context, externalMessage);
                }

                enabledTransitions = SelectTransitions(root, context, externalMessage);

                if (!enabledTransitions.IsEmpty())
                {
                    await Microstep(root, context, enabledTransitions);
                }

                context.LogInformation("End: event loop cycle");
            }

            foreach (var state in context.Configuration.Sort(State.ReverseCompare))
            {
                await state.Exit(context);

                if (state.IsFinalState)
                {
                    if (state.Parent.IsScxmlRoot)
                    {
                        ReturnDoneMessage(state);
                    }
                }
            }

            context.LogInformation("End: event loop");
        }

        private void ReturnDoneMessage(State state)
        {
            // The implementation of returnDoneMessage is platform-dependent, but if this session is the result of an <invoke> in another SCXML session, 
            //  returnDoneMessage will cause the event done.invoke.<id> to be placed in the external event queue of that session, where <id> is the id 
            //  generated in that session when the <invoke> was executed.
        }

        private async Task Microstep(RootState root, ExecutionContext context, IEnumerable<Transition> enabledTransitions)
        {
            Debug.Assert(enabledTransitions != null);

            await ExitStates(root, context, enabledTransitions);
            
            foreach (var transition in enabledTransitions)
            {
                Debug.Assert(transition != null);

                await transition.ExecuteContent(context);
            }

            await EnterStates(root, context, enabledTransitions);
        }

        private async Task ExitStates(RootState root, ExecutionContext context, IEnumerable<Transition> enabledTransitions)
        {
            var exitSet = ComputeExitSet(root, context, enabledTransitions);

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

                await state.Exit(context);
            }
        }

        private Set<State> ComputeExitSet(RootState root,
                                          ExecutionContext context,
                                          IEnumerable<Transition> transitions)
        {
            Debug.Assert(root != null);
            Debug.Assert(context != null);
            Debug.Assert(transitions != null);

            var statesToExit = new Set<State>();

            foreach (var transition in transitions)
            {
                Debug.Assert(transition != null);

                if (transition.HasTargets)
                {
                    var domain = transition.GetTransitionDomain(context, root);

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

        private Set<Transition> SelectTransitions(RootState root, ExecutionContext context, Message evt)
        {
            return SelectTransitions(root,
                                     context,
                                     transition => transition.MatchesMessage(evt) &&
                                                   transition.EvaluateCondition(context));
        }

        private Set<Transition> SelectMessagelessTransitions(RootState root, ExecutionContext context)
        {
            return SelectTransitions(root,
                                     context,
                                     transition => !transition.HasMessage &&
                                                   transition.EvaluateCondition(context));
        }

        private Set<Transition> SelectTransitions(RootState root, ExecutionContext context, Func<Transition, bool> predicate)
        {
            Debug.Assert(root != null);
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

                foreach (var anc in state.GetProperAncestors(root))
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

            enabledTransitions = RemoveConflictingTransitions(root, context, enabledTransitions);

            Debug.Assert(enabledTransitions != null);

            return enabledTransitions;
        }

        private Set<Transition> RemoveConflictingTransitions(RootState root,
                                                             ExecutionContext context,
                                                             IEnumerable<Transition> enabledTransitions)
        {
            Debug.Assert(root != null);
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

                    var exitSet1 = ComputeExitSet(root, context, new List<Transition> { transition1 });

                    Debug.Assert(exitSet1 != null);

                    var exitSet2 = ComputeExitSet(root, context, new List<Transition> { transition2 });

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

        private async Task EnterStates(RootState root,
                                       ExecutionContext context,
                                       IEnumerable<Transition> enabledTransitions)
        {
            var statesToEnter = new Set<State>();

            var statesForDefaultEntry = new Set<State>();

            var defaultHistoryContent = new Dictionary<string, Set<ExecutableContent>>();

            await ComputeEntrySet(root, context, enabledTransitions, statesToEnter, statesForDefaultEntry, defaultHistoryContent);

            foreach (var state in statesToEnter.Sort(State.Compare))
            {
                Debug.Assert(state != null);

                await state.Enter(context, root, statesForDefaultEntry, defaultHistoryContent);
            }
        }

        private async Task ComputeEntrySet(RootState root,
                                           ExecutionContext context,
                                           IEnumerable<Transition> enabledTransitions,
                                           Set<State> statesToEnter,
                                           Set<State> statesForDefaultEntry,
                                           Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            Debug.Assert(root != null);
            Debug.Assert(context != null);
            Debug.Assert(enabledTransitions != null);

            foreach (var transition in enabledTransitions)
            {
                Debug.Assert(transition != null);

                foreach (var state in transition.GetTargetStates(root))
                {
                    Debug.Assert(state != null);

                    await AddDescendentStatesToEnter(root, context, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }

                var ancestor = transition.GetTransitionDomain(context, root);

                var effectiveTargetStates = transition.GetEffectiveTargetStates(context, root);

                Debug.Assert(effectiveTargetStates != null);

                foreach (var state in effectiveTargetStates)
                {
                    Debug.Assert(state != null);

                    await AddAncestorStatesToEnter(root, context, state, ancestor, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                }
            }
        }

        private async Task AddAncestorStatesToEnter(RootState root,
                                                    ExecutionContext context,
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
                            await AddDescendentStatesToEnter(root, context, child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }

        private async Task AddDescendentStatesToEnter(RootState root,
                                                      ExecutionContext context,
                                                      State state,
                                                      Set<State> statesToEnter,
                                                      Set<State> statesForDefaultEntry,
                                                      Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            Debug.Assert(root != null);
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

                        await AddDescendentStatesToEnter(root, context, resolved, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var resolved in resolvedStates)
                    {
                        Debug.Assert(resolved != null);

                        await AddAncestorStatesToEnter(root, context, resolved, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }
                }
                else
                {
                    var targetStates = new List<State>();

                    ((HistoryState) state).VisitTransition(targetStates, defaultHistoryContent, root);

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddDescendentStatesToEnter(root, context, target, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddAncestorStatesToEnter(root, context, target, state.Parent, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
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

                    var targetStates = initialTransition.GetTargetStates(root);

                    Debug.Assert(targetStates != null);

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddDescendentStatesToEnter(root, context, target, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                    }

                    foreach (var target in targetStates)
                    {
                        Debug.Assert(target != null);

                        await AddAncestorStatesToEnter(root, context, target, state, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
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
                            await AddDescendentStatesToEnter(root, context, child, statesToEnter, statesForDefaultEntry, defaultHistoryContent);
                        }
                    }
                }
            }
        }
    }
}
