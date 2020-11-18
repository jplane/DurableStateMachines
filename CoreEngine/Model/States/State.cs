using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.CoreEngine.Model.Execution;
using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal abstract class State
    {
        protected readonly IStateMetadata _metadata;
        protected readonly State _parent;
        protected readonly Lazy<OnEntryExit> _onEntry;
        protected readonly Lazy<OnEntryExit> _onExit;
        protected readonly Lazy<Transition[]> _transitions;
        protected readonly Lazy<InvokeStateChart[]> _invokes;
        protected readonly Lazy<Datamodel> _datamodel;

        private bool _firstEntry;

        protected State(IStateMetadata metadata, State parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _firstEntry = true;
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
                return _metadata.GetServices().Select(sm => new InvokeStateChart(sm, this)).ToArray();
            });

            _datamodel = new Lazy<Datamodel>(() =>
            {
                var meta = _metadata.GetDatamodel();

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

        public IEnumerable<Transition> GetTransitions()
        {
            return _transitions.Value;
        }

        public virtual Transition GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }

        public virtual void InitDatamodel(ExecutionContext context, bool recursive)
        {
            _datamodel.Value?.Init(context);
        }

        public virtual async Task Invoke(ExecutionContext context)
        {
            foreach (var invoke in _invokes.Value)
            {
                await invoke.Execute(context);
            }
        }

        public virtual void RecordHistory(ExecutionContext context)
        {
        }

        public virtual IEnumerable<State> GetChildStates()
        {
            return Enumerable.Empty<State>();
        }

        public virtual bool IsInFinalState(ExecutionContext context)
        {
            return false;
        }

        public virtual State GetState(string id)
        {
            if (string.Compare(id, this.Id, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        public async Task ProcessExternalMessage(ExecutionContext context, Message evt)
        {
            foreach (var invoke in _invokes.Value)
            {
                await invoke.ProcessExternalMessage(context, evt);
            }
        }

        public Set<State> GetEffectiveTargetStates(ExecutionContext context)
        {
            var set = new Set<State>();

            foreach (var transition in _transitions.Value)
            {
                var transitionSet = transition.GetEffectiveTargetStates(context);

                set.Union(transitionSet);
            }

            return set;
        }

        public async Task Enter(ExecutionContext context,
                                Set<State> statesForDefaultEntry,
                                Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            context.CheckArgNull(nameof(context));
            statesForDefaultEntry.CheckArgNull(nameof(statesForDefaultEntry));
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            context.LogInformation($"Enter {this.GetType().Name}: Id {this.Id}");

            context.Configuration.Add(this);

            context.StatesToInvoke.Add(this);

            if (context.Root.Binding == Databinding.Late && _firstEntry)
            {
                this.InitDatamodel(context, false);

                _firstEntry = false;
            }

            _onEntry.Value?.Execute(context);

            if (statesForDefaultEntry.Contains(this))
            {
                var transition = this.GetInitialStateTransition();

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

        public async Task Exit(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation($"Exit {this.GetType().Name}: Id {this.Id}");

            _onExit.Value?.Execute(context);

            foreach (var invoke in _invokes.Value)
            {
                await invoke.Cancel(context);
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
