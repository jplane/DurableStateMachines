using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.Model.Execution;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common;
using System.Diagnostics;
using StateChartsDotNet.Common.Debugger;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateChartsDotNet.Model.States
{
    internal class State<TData>
    {
        protected readonly IStateMetadata _metadata;
        protected readonly State<TData> _parent;
        protected readonly Lazy<OnEntryExit<TData>> _onEntry;
        protected readonly Lazy<OnEntryExit<TData>> _onExit;
        protected readonly Lazy<Transition<TData>[]> _transitions;
        protected readonly Lazy<InvokeStateChart<TData>[]> _invokes;
        protected readonly Lazy<State<TData>[]> _states;
        protected readonly Lazy<Transition<TData>> _initialTransition;

        public State(IStateMetadata metadata, State<TData> parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _parent = parent;

            _onEntry = new Lazy<OnEntryExit<TData>>(() =>
            {
                var meta = _metadata.GetOnEntry();

                if (meta != null)
                    return new OnEntryExit<TData>(meta);
                else
                    return null;
            });

            _onExit = new Lazy<OnEntryExit<TData>>(() =>
            {
                var meta = _metadata.GetOnExit();

                if (meta != null)
                    return new OnEntryExit<TData>(meta);
                else
                    return null;
            });

            _transitions = new Lazy<Transition<TData>[]>(() =>
            {
                return _metadata.GetTransitions().Select(tm => new Transition<TData>(tm, this)).ToArray();
            });

            _invokes = new Lazy<InvokeStateChart<TData>[]>(() =>
            {
                return _metadata.GetStateChartInvokes().Select(sm => new InvokeStateChart<TData>(sm, _metadata.MetadataId)).ToArray();
            });

            _initialTransition = new Lazy<Transition<TData>>(() =>
            {
                var meta = metadata.GetInitialTransition();

                if (meta != null)
                    return new Transition<TData>(meta, this);
                else
                    return null;
            });

            _states = new Lazy<State<TData>[]>(() =>
            {
                var states = new List<State<TData>>();

                foreach (var stateMetadata in metadata.GetStates())
                {
                    switch (stateMetadata.Type)
                    {
                        case StateType.Atomic:
                        case StateType.Compound:
                        case StateType.Parallel:
                            states.Add(new State<TData>(stateMetadata, this));
                            break;

                        case StateType.History:
                            states.Add(new HistoryState<TData>((IHistoryStateMetadata) stateMetadata, this));
                            break;

                        case StateType.Final:
                            states.Add(new FinalState<TData>((IFinalStateMetadata) stateMetadata, this));
                            break;
                    }
                }

                return states.ToArray();
            });
        }

        public virtual string Id => _metadata.Id;

        public State<TData> Parent => _parent;

        public StateType Type => _metadata.Type;

        public IEnumerable<Transition<TData>> GetTransitions()
        {
            return _transitions.Value;
        }

        public virtual Transition<TData> GetInitialStateTransition()
        {
            return _initialTransition.Value;
        }

        public virtual async Task InvokeAsync(ExecutionContextBase<TData> context)
        {
            foreach (var invoke in _invokes.Value)
            {
                await invoke.ExecuteAsync(context);
            }
        }

        public virtual void RecordHistory(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            foreach (var history in _states.Value.OfType<HistoryState<TData>>())
            {
                Func<State<TData>, bool> predicate;

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

        public virtual IEnumerable<State<TData>> GetChildStates()
        {
            Debug.Assert(_states != null);

            return _states.Value.Where(s => ! (s is HistoryState<TData>));
        }

        public bool IsInFinalState(ExecutionContextBase<TData> context)
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

        public State<TData> GetState(string id)
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

        public Set<State<TData>> GetEffectiveTargetStates(ExecutionContextBase<TData> context)
        {
            var set = new Set<State<TData>>();

            foreach (var transition in _transitions.Value)
            {
                var transitionSet = transition.GetEffectiveTargetStates(context);

                set.Union(transitionSet);
            }

            return set;
        }

        public async Task EnterAsync(ExecutionContextBase<TData> context,
                                     Set<State<TData>> statesForDefaultEntry,
                                     Dictionary<string, Set<ExecutableContent<TData>>> defaultHistoryContent)
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

            if (defaultHistoryContent.TryGetValue(this.Id, out Set<ExecutableContent<TData>> set))
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

        public async Task ExitAsync(ExecutionContextBase<TData> context)
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

        public bool IsDescendent(State<TData> state)
        {
            state.CheckArgNull(nameof(state));

            return _metadata.IsDescendentOf(state._metadata);
        }

        public static int Compare(State<TData> state1, State<TData> state2)
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

        public static int ReverseCompare(State<TData> state1, State<TData> state2)
        {
            return Compare(state1, state2) * -1;
        }

        public IEnumerable<State<TData>> GetProperAncestors(State<TData> state = null)
        {
            var set = new List<State<TData>>();

            Func<State<TData>, bool> predicate = _ => true;

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
