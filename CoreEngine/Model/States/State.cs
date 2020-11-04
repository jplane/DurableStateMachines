using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using CoreEngine.Model.Execution;
using CoreEngine.Model.DataManipulation;

namespace CoreEngine.Model.States
{
    internal abstract class State
    {
        protected readonly XElement _element;
        protected readonly State _parent;
        protected readonly Lazy<OnEntryExit> _onEntry;
        protected readonly Lazy<OnEntryExit> _onExit;
        protected readonly Lazy<SCG.List<Transition>> _transitions;
        protected readonly Lazy<SCG.List<Invoke>> _invokes;
        protected readonly Lazy<Datamodel> _datamodel;

        private bool _firstEntry;

        protected State(XElement element, State parent)
        {
            _firstEntry = true;
            _element = element;
            _parent = parent;

            _onEntry = new Lazy<OnEntryExit>(() =>
            {
                var node = element.Element("onentry");

                return node == null ? null : new OnEntryExit(node);
            });

            _onExit = new Lazy<OnEntryExit>(() =>
            {
                var node = element.Element("onexit");

                return node == null ? null : new OnEntryExit(node);
            });

            _transitions = new Lazy<SCG.List<Transition>>(() =>
            {
                var nodes = element.Elements("transition");

                return new SCG.List<Transition>(nodes.Select(n => new Transition(n, this)));
            });

            _invokes = new Lazy<SCG.List<Invoke>>(() =>
            {
                var nodes = element.Elements("invoke");

                return new SCG.List<Invoke>(nodes.Select(n => new Invoke(n)));
            });

            _datamodel = new Lazy<Datamodel>(() =>
            {
                var node = element.Element("datamodel");

                return node == null ? null : new Datamodel(node);
            });
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

        public List<Transition> Transitions => new List<Transition>(_transitions.Value);

        public virtual Transition GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }

        public virtual void InitDatamodel(ExecutionContext context, bool recursive)
        {
            _datamodel.Value.Init(context);
        }

        public static int GetDocumentOrder(State state1, State state2)
        {
            return state1.GetRelativeOrder(state2);
        }

        public static int GetReverseDocumentOrder(State state1, State state2)
        {
            return state1.GetRelativeOrder(state2, true);
        }

        public int GetRelativeOrder(State state, bool bottomUp = false)
        {
            if (state == null)
            {
                return 1;
            }
            else
            {
                bool AreEqual(string x, string y)
                {
                    return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0;
                }

                var ids = _element.Document.Descendants()
                                           .Where(e => e.Name == "state" || e.Name == "parallel" || e.Name == "final")
                                           .Select(e => e.Attribute("id").Value);
                
                if (bottomUp)
                {
                    ids = ids.Reverse();
                }

                var result = ids.FirstOrDefault(id => AreEqual(id, this.Id) || AreEqual(id, state.Id));

                if (string.IsNullOrWhiteSpace(result))
                {
                    return 0;
                }
                else
                {
                    return AreEqual(result, this.Id) ? 1 : -1;
                }
            }
        }

        public virtual void Invoke(ExecutionContext context, RootState root)
        {
            foreach (var invoke in _invokes.Value)
            {
                invoke.Execute(context);
            }
        }

        public virtual void RecordHistory(ExecutionContext context, RootState root)
        {
        }

        public virtual List<State> GetChildStates()
        {
            return new List<State>();
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

        public OrderedSet<State> GetEffectiveTargetStates(ExecutionContext context, RootState root)
        {
            var set = new OrderedSet<State>();

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
                          OrderedSet<State> statesForDefaultEntry,
                          SCG.Dictionary<string, OrderedSet<ExecutableContent>> defaultHistoryContent)
        {
            context.Configuration.Add(this);

            context.StatesToInvoke.Add(this);

            if (root.Binding == Databinding.Late && _firstEntry)
            {
                this.InitDatamodel(context, false);

                _firstEntry = false;
            }

            _onEntry.Value.Execute(context);

            if (statesForDefaultEntry.IsMember(this))
            {
                var transition = this.GetInitialStateTransition();

                transition.ExecuteContent(context);
            }

            if (defaultHistoryContent.TryGetValue(this.Id, out OrderedSet<ExecutableContent> set))
            {
                foreach (var content in set.ToList())
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

                        if (parallelChildren.Every(s => s.IsInFinalState(context, root)))
                        {
                            context.EnqueueInternal("done.state." + grandparent.Id);
                        }
                    }
                }
            }
        }

        public void Exit(ExecutionContext context)
        {
            _onExit.Value.Execute(context);

            CancelOutstandingInvokes(context);

            context.Configuration.Delete(this);
        }

        public bool IsDescendent(State state)
        {
            return state._element.Descendants().Contains(this._element);
        }

        public List<State> GetProperAncestors(State state = null)
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
                set.Append(parent);

                parent = parent.Parent;
            }

            return set;
        }
    }
}
