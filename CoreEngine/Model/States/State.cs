using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using CoreEngine.Model.Execution;
using CoreEngine.Model.DataManipulation;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CoreEngine.Model.States
{
    internal abstract class State
    {
        protected readonly XElement _element;
        protected readonly State _parent;
        protected readonly Lazy<OnEntryExit> _onEntry;
        protected readonly Lazy<OnEntryExit> _onExit;
        protected readonly Lazy<List<Transition>> _transitions;
        protected readonly Lazy<List<Invoke>> _invokes;
        protected readonly Lazy<Datamodel> _datamodel;

        private bool _firstEntry;

        protected State(XElement element, State parent)
        {
            element.CheckArgNull(nameof(element));

            _firstEntry = true;
            _element = element;
            _parent = parent;

            _onEntry = new Lazy<OnEntryExit>(() =>
            {
                var node = element.ScxmlElement("onentry");

                return node == null ? null : new OnEntryExit(node);
            });

            _onExit = new Lazy<OnEntryExit>(() =>
            {
                var node = element.ScxmlElement("onexit");

                return node == null ? null : new OnEntryExit(node);
            });

            _transitions = new Lazy<List<Transition>>(() =>
            {
                var nodes = element.ScxmlElements("transition");

                return new List<Transition>(nodes.Select(n => new Transition(n, this)));
            });

            _invokes = new Lazy<List<Invoke>>(() =>
            {
                var nodes = element.ScxmlElements("invoke");

                return new List<Invoke>(nodes.Select(n => new Invoke(n)));
            });

            _datamodel = new Lazy<Datamodel>(() =>
            {
                var node = element.ScxmlElement("datamodel");

                return node == null ? null : new Datamodel(node);
            });
        }

        public static XObject GetXObject(State state)
        {
            state.CheckArgNull(nameof(state));

            return state._element;
        }

        public virtual string Id => _element.Attribute("id")?.Value ?? string.Empty;

        public State Parent => _parent;

        public virtual bool IsScxmlRoot => false;

        public virtual bool IsFinalState => false;

        public virtual bool IsHistoryState => false;

        public virtual bool IsDeepHistoryState => false;

        public virtual bool IsSequentialState => false;

        public virtual bool IsParallelState => false;

        public virtual bool IsAtomic => false;

        public IEnumerable<Transition> Transitions => _transitions.Value;

        public virtual Transition GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }

        public virtual Task InitDatamodel(ExecutionContext context, bool recursive)
        {
            return _datamodel.Value == null ? Task.CompletedTask : _datamodel.Value.Init(context);
        }

        public virtual void Invoke(ExecutionContext context, RootState root)
        {
            foreach (var invoke in _invokes.Value)
            {
                invoke.Execute(context);
            }
        }

        public virtual void RecordHistory(ExecutionContext context)
        {
        }

        public virtual IEnumerable<State> GetChildStates()
        {
            return Enumerable.Empty<State>();
        }

        public virtual bool IsInFinalState(ExecutionContext context, RootState root)
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

        public void ProcessExternalEvent(ExecutionContext context, Event evt)
        {
            foreach (var invoke in _invokes.Value)
            {
                invoke.ProcessExternalEvent(context, evt);
            }
        }

        public Set<State> GetEffectiveTargetStates(ExecutionContext context, RootState root)
        {
            var set = new Set<State>();

            foreach (var transition in _transitions.Value)
            {
                var transitionSet = transition.GetEffectiveTargetStates(context, root);

                set.Union(transitionSet);
            }

            return set;
        }

        private void CancelOutstandingInvokes(ExecutionContext context)
        {
            foreach (var invoke in _invokes.Value)
            {
                invoke.Cancel(context);
            }
        }

        public void Enter(ExecutionContext context,
                          RootState root,
                          Set<State> statesForDefaultEntry,
                          Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            context.CheckArgNull(nameof(context));
            root.CheckArgNull(nameof(root));
            statesForDefaultEntry.CheckArgNull(nameof(statesForDefaultEntry));
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            context.Configuration.Add(this);

            context.StatesToInvoke.Add(this);

            if (root.Binding == Databinding.Late && _firstEntry)
            {
                this.InitDatamodel(context, false);

                _firstEntry = false;
            }

            _onEntry.Value?.Execute(context);

            if (statesForDefaultEntry.Contains(this))
            {
                var transition = this.GetInitialStateTransition();

                transition.ExecuteContent(context);
            }

            if (defaultHistoryContent.TryGetValue(this.Id, out Set<ExecutableContent> set))
            {
                foreach (var content in set)
                {
                    content.Execute(context);
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

                        if (parallelChildren.All(s => s.IsInFinalState(context, root)))
                        {
                            context.EnqueueInternal("done.state." + grandparent.Id);
                        }
                    }
                }
            }
        }

        public void Exit(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            _onExit.Value?.Execute(context);

            CancelOutstandingInvokes(context);

            context.Configuration.Remove(this);
        }

        public bool IsDescendent(State state)
        {
            state.CheckArgNull(nameof(state));

            return state._element.Descendants().Contains(this._element);
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
