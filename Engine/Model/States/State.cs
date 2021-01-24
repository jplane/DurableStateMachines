using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.Model.Execution;
using StateChartsDotNet.Model.Data;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common;
using System.Diagnostics;
using StateChartsDotNet.Common.Debugger;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateChartsDotNet.Model.States
{
    internal class State
    {
        protected readonly IStateMetadata _metadata;
        protected readonly State _parent;
        protected readonly Lazy<OnEntryExit> _onEntry;
        protected readonly Lazy<OnEntryExit> _onExit;
        protected readonly Lazy<Transition[]> _transitions;
        protected readonly Lazy<InvokeStateChart[]> _invokes;
        protected readonly Lazy<State[]> _states;
        protected readonly Lazy<Transition> _initialTransition;

        public State(IStateMetadata metadata, State parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _parent = parent;

            _onEntry = new Lazy<OnEntryExit>(() =>
            {
                var meta = _metadata.GetOnEntry();

                if (meta != null)
                    return new OnEntryExit(meta);
                else
                    return null;
            });

            _onExit = new Lazy<OnEntryExit>(() =>
            {
                var meta = _metadata.GetOnExit();

                if (meta != null)
                    return new OnEntryExit(meta);
                else
                    return null;
            });

            _transitions = new Lazy<Transition[]>(() =>
            {
                return _metadata.GetTransitions().Select(tm => new Transition(tm, this)).ToArray();
            });

            _invokes = new Lazy<InvokeStateChart[]>(() =>
            {
                return _metadata.GetStateChartInvokes().Select(sm => new InvokeStateChart(sm, _metadata.MetadataId)).ToArray();
            });

            _initialTransition = new Lazy<Transition>(() =>
            {
                var meta = metadata.GetInitialTransition();

                if (meta != null)
                    return new Transition(meta, this);
                else
                    return null;
            });

            _states = new Lazy<State[]>(() =>
            {
                var states = new List<State>();

                foreach (var stateMetadata in metadata.GetStates())
                {
                    switch (stateMetadata.Type)
                    {
                        case StateType.Atomic:
                        case StateType.Compound:
                        case StateType.Parallel:
                            states.Add(new State(stateMetadata, this));
                            break;

                        case StateType.History:
                            states.Add(new HistoryState((IHistoryStateMetadata) stateMetadata, this));
                            break;

                        case StateType.Final:
                            states.Add(new FinalState((IFinalStateMetadata) stateMetadata, this));
                            break;
                    }
                }

                return states.ToArray();
            });
        }

        public virtual string Id => _metadata.Id;

        public State Parent => _parent;

        public StateType Type => _metadata.Type;

        public IEnumerable<Transition> GetTransitions()
        {
            return _transitions.Value;
        }

        public virtual Transition GetInitialStateTransition()
        {
            return _initialTransition.Value;
        }

        public virtual async Task InvokeAsync(ExecutionContextBase context)
        {
            foreach (var invoke in _invokes.Value)
            {
                await invoke.ExecuteAsync(context);
            }
        }

        public virtual void RecordHistory(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            foreach (var history in _states.Value.OfType<HistoryState>())
            {
                Func<State, bool> predicate;

                if (history.IsDeep)
                {
                    predicate = s => s.Type == StateType.Atomic && s.IsDescendent(this);
                }
                else
                {
                    predicate = s => string.Compare(_parent.Id, this.Id, StringComparison.InvariantCultureIgnoreCase) == 0;
                }

                context.StoreHistoryValue(history.Id, predicate);
            }
        }

        public virtual IEnumerable<State> GetChildStates()
        {
            Debug.Assert(_states != null);

            return _states.Value.Where(s => ! (s is HistoryState));
        }

        public bool IsInFinalState(ExecutionContextBase context)
        {
            if (_metadata.Type == StateType.Parallel)
            {
                foreach (var child in GetChildStates())
                {
                    if (!child.IsInFinalState(context))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                foreach (var child in GetChildStates())
                {
                    if (child.IsInFinalState(context) && context.Configuration.Contains(child))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public State GetState(string id)
        {
            if (string.Compare(id, this.Id, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return this;
            }
            else
            {
                foreach (var state in GetChildStates())
                {
                    var result = state.GetState(id);

                    if (result != null)
                    {
                        return result;
                    }
                }

                return null;
            }
        }

        public Set<State> GetEffectiveTargetStates(ExecutionContextBase context)
        {
            var set = new Set<State>();

            foreach (var transition in _transitions.Value)
            {
                var transitionSet = transition.GetEffectiveTargetStates(context);

                set.Union(transitionSet);
            }

            return set;
        }

        public async Task EnterAsync(ExecutionContextBase context,
                                     Set<State> statesForDefaultEntry,
                                     Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            context.CheckArgNull(nameof(context));
            statesForDefaultEntry.CheckArgNull(nameof(statesForDefaultEntry));
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            await context.LogInformationAsync($"Enter {this.GetType().Name}: Id {this.Id}");

            await context.BreakOnDebugger(DebuggerAction.EnterState, _metadata);

            context.Configuration.Add(this);

            context.StatesToInvoke.Add(this);

            if (_onEntry.Value != null)
            {
                await _onEntry.Value.ExecuteAsync(context);
            }

            if (statesForDefaultEntry.Contains(this))
            {
                var transition = this.GetInitialStateTransition();

                if (transition != null)
                {
                    await transition.ExecuteContentAsync(context);
                }
            }

            if (defaultHistoryContent.TryGetValue(this.Id, out Set<ExecutableContent> set))
            {
                foreach (var content in set)
                {
                    await content.ExecuteAsync(context);
                }
            }

            if (this.Type == StateType.Final)
            {
                if (_parent.Type == StateType.Root)
                {
                    context.EnterFinalRootState();
                }
                else
                {
                    context.EnqueueInternal("done.state." + _parent.Id);

                    var grandparent = _parent?.Parent;

                    if (grandparent != null && grandparent.Type == StateType.Parallel)
                    {
                        var parallelChildren = grandparent.GetChildStates();

                        var allInFinalState = true;

                        foreach (var pc in parallelChildren)
                        {
                            if (! pc.IsInFinalState(context))
                            {
                                allInFinalState = false;
                                break;
                            }
                        }

                        if (allInFinalState)
                        {
                            context.EnqueueInternal("done.state." + grandparent.Id);
                        }
                    }
                }
            }
        }

        public async Task ExitAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync($"Exit {this.GetType().Name}: Id {this.Id}");

            if (_onExit.Value != null)
            {
                await _onExit.Value.ExecuteAsync(context);
            }

            context.Configuration.Remove(this);

            await context.BreakOnDebugger(DebuggerAction.ExitState, _metadata);
        }

        public bool IsDescendent(State state)
        {
            state.CheckArgNull(nameof(state));

            return _metadata.IsDescendentOf(state._metadata);
        }

        public static int Compare(State state1, State state2)
        {
            if (state1 == null && state2 == null)
            {
                return 0;
            }
            else if (state1 == null)
            {
                return -1;
            }
            else if (state2 == null)
            {
                return 1;
            }
            else
            {
                var state1DocOrder = state1._metadata.GetDocumentOrder();
                var state2DocOrder = state2._metadata.GetDocumentOrder();

                return state1DocOrder == state2DocOrder ? 0 : state1DocOrder > state2DocOrder ? -1 : 1;
            }
        }

        public static int ReverseCompare(State state1, State state2)
        {
            return Compare(state1, state2) * -1;
        }

        public IEnumerable<State> GetProperAncestors(State state = null)
        {
            var set = new List<State>();

            Func<State, bool> predicate = _ => true;

            if (state != null)
            {
                if (this.Id == state.Id || state.IsDescendent(this))
                {
                    return set;
                }

                predicate = s => s.Id != state.Id;
            }

            var parent = _parent;

            while (parent != null && predicate(parent))
            {
                set.Add(parent);

                parent = parent.Parent;
            }

            return set;
        }
    }
}
