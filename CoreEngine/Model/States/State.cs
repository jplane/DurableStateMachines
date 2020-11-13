using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.CoreEngine.Model.Execution;
using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal abstract class State
    {
        protected readonly IStateMetadata _metadata;
        protected readonly State _parent;
        protected readonly AsyncLazy<OnEntryExit> _onEntry;
        protected readonly AsyncLazy<OnEntryExit> _onExit;
        protected readonly AsyncLazy<Transition[]> _transitions;
        protected readonly AsyncLazy<Service[]> _invokes;
        protected readonly AsyncLazy<Datamodel> _datamodel;

        private bool _firstEntry;

        protected State(IStateMetadata metadata, State parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _firstEntry = true;
            _metadata = metadata;
            _parent = parent;

            _onEntry = new AsyncLazy<OnEntryExit>(async () =>
            {
                var meta = await _metadata.GetOnEntry();

                if (meta != null)
                    return new OnEntryExit(meta);
                else
                    return null;
            });

            _onExit = new AsyncLazy<OnEntryExit>(async () =>
            {
                var meta = await _metadata.GetOnExit();

                if (meta != null)
                    return new OnEntryExit(meta);
                else
                    return null;
            });

            _transitions = new AsyncLazy<Transition[]>(async () =>
            {
                return (await _metadata.GetTransitions()).Select(tm => new Transition(tm, this)).ToArray();
            });

            _invokes = new AsyncLazy<Service[]>(async () =>
            {
                return (await _metadata.GetServices()).Select(sm => new Service(sm, this)).ToArray();
            });

            _datamodel = new AsyncLazy<Datamodel>(async () =>
            {
                var meta = await _metadata.GetDatamodel();

                if (meta != null)
                    return new Datamodel(meta);
                else
                    return null;
            });
        }

        public virtual string Id => _metadata.Id;

        public State Parent => _parent;

        public virtual bool IsScxmlRoot => false;

        public virtual bool IsFinalState => false;

        public virtual bool IsHistoryState => false;

        public virtual bool IsDeepHistoryState => false;

        public virtual bool IsSequentialState => false;

        public virtual bool IsParallelState => false;

        public virtual bool IsAtomic => false;

        public async Task<IEnumerable<Transition>> GetTransitions()
        {
            return await _transitions;
        }

        public virtual Task<Transition> GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }

        public virtual async Task InitDatamodel(ExecutionContext context, bool recursive)
        {
            var datamodel = await _datamodel;

            if (datamodel != null)
            {
                await datamodel.Init(context);
            }
        }

        public virtual async Task Invoke(ExecutionContext context, RootState root)
        {
            foreach (var invoke in await _invokes)
            {
                await invoke.Execute(context);
            }
        }

        public virtual Task RecordHistory(ExecutionContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task<IEnumerable<State>> GetChildStates()
        {
            return Task.FromResult(Enumerable.Empty<State>());
        }

        public virtual Task<bool> IsInFinalState(ExecutionContext context, RootState root)
        {
            return Task.FromResult(false);
        }

        public virtual Task<State> GetState(string id)
        {
            if (string.Compare(id, this.Id, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return Task.FromResult(this);
            }
            else
            {
                return Task.FromResult<State>(null);
            }
        }

        public async Task ProcessExternalEvent(ExecutionContext context, Event evt)
        {
            foreach (var invoke in await _invokes)
            {
                await invoke.ProcessExternalEvent(context, evt);
            }
        }

        public async Task<Set<State>> GetEffectiveTargetStates(ExecutionContext context, RootState root)
        {
            var set = new Set<State>();

            foreach (var transition in await _transitions)
            {
                var transitionSet = await transition.GetEffectiveTargetStates(context, root);

                set.Union(transitionSet);
            }

            return set;
        }

        public async Task Enter(ExecutionContext context,
                                RootState root,
                                Set<State> statesForDefaultEntry,
                                Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            context.CheckArgNull(nameof(context));
            root.CheckArgNull(nameof(root));
            statesForDefaultEntry.CheckArgNull(nameof(statesForDefaultEntry));
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            context.LogInformation($"Enter {this.GetType().Name}: Id {this.Id}");

            context.Configuration.Add(this);

            context.StatesToInvoke.Add(this);

            if (root.Binding == Databinding.Late && _firstEntry)
            {
                await this.InitDatamodel(context, false);

                _firstEntry = false;
            }

            var onEntry = await _onEntry;

            if (onEntry != null)
            { 
                await onEntry.Execute(context);
            }

            if (statesForDefaultEntry.Contains(this))
            {
                var transition = await this.GetInitialStateTransition();

                if (transition != null)
                {
                    await transition.ExecuteContent(context);
                }
            }

            if (defaultHistoryContent.TryGetValue(this.Id, out Set<ExecutableContent> set))
            {
                foreach (var content in set)
                {
                    await content.Execute(context);
                }
            }

            if (this.IsFinalState)
            {
                if (_parent.IsScxmlRoot)
                {
                    context.IsRunning = false;
                }
                else
                {
                    context.EnqueueInternal("done.state." + this.Id);

                    var grandparent = _parent?.Parent;

                    if (grandparent != null && grandparent.IsParallelState)
                    {
                        var parallelChildren = await grandparent.GetChildStates();

                        var allInFinalState = true;

                        foreach (var pc in parallelChildren)
                        {
                            if (! await pc.IsInFinalState(context, root))
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

        public async Task Exit(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation($"Exit {this.GetType().Name}: Id {this.Id}");

            var onExit = await _onExit;

            if (onExit != null)
            {
                await onExit.Execute(context);
            }

            foreach (var invoke in await _invokes)
            {
                invoke.Cancel(context);
            }

            context.Configuration.Remove(this);
        }

        public bool IsDescendent(State state)
        {
            state.CheckArgNull(nameof(state));

            return _metadata.IsDescendentOf(state._metadata);
        }

        public static int Compare(State state1, State state2)
        {
            return state1._metadata.DepthFirstCompare(state2._metadata);
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
