using System;
using System.Linq;
using System.Xml.Linq;
using SCG = System.Collections.Generic;

namespace CoreEngine.Model
{
    internal class _State
    {
        private readonly XElement _element;
        private readonly Lazy<OnEntryExit> _onEntry;
        private readonly Lazy<OnEntryExit> _onExit;
        private readonly Lazy<List<Transition>> _transitions;
        private readonly Lazy<List<Invoke>> _invokes;
        private readonly Lazy<List<_State>> _history;

        private bool _firstEntry = true;

        public _State(XElement element)
        {
            _element = element;

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

            _transitions = new Lazy<List<Transition>>(() =>
            {
                var nodes = element.Elements("transition");

                return new List<Transition>(nodes.Select(n => new Transition(n, this)));
            });

            _invokes = new Lazy<List<Invoke>>(() =>
            {
                var nodes = element.Elements("invoke");

                return new List<Invoke>(nodes.Select(n => new Invoke(n, this)));
            });

            _history = new Lazy<List<_State>>(() =>
            {
                var nodes = element.Elements("history");

                return new List<_State>(nodes.Select(n => new _State(n)));
            });
        }

        public List<Transition> Transitions => _transitions.Value;

        public List<Invoke> Invokes => _invokes.Value;

        public string Id
        {
            get
            {
                var attr = _element.Attribute("id");

                if (attr != null)
                {
                    return attr.Value;
                }

                attr = _element.Attribute("name");

                if (attr != null)
                {
                    return attr.Value;
                }

                return "scxml_root";
            }
        }

        public bool IsScxmlRoot => _element.Name == "scxml";

        public bool IsFinalState => _element.Name == "final";

        public bool IsHistoryState => _element.Name == "history";

        private bool IsDeepHistoryState
        {
            get
            {
                if (! IsHistoryState)
                {
                    return false;
                }

                var type = _element.Attribute("type")?.Value;

                return string.Compare(type, "deep", StringComparison.InvariantCultureIgnoreCase) == 0;
            }
        }

        public bool IsCompoundState => _element.Name == "state" &&
                                       _element.Elements().Count(e => StateChart.IsStateElement(e)) > 0;

        public bool IsParallelState => _element.Name == "parallel";

        public bool IsAtomic => _element.Name == "state" && !IsCompoundState;

        public void Invoke(ExecutionContext context, StateChart statechart)
        {
        }

        public OrderedSet<_State> GetEffectiveTargetStates(ExecutionContext context, StateChart statechart)
        {
            var set = new OrderedSet<_State>();

            foreach (var transition in _transitions.Value)
            {
                var transitionSet = transition.GetEffectiveTargetStates(context, statechart);

                set.Union(transitionSet);
            }

            return set;
        }

        private void CancelOutstandingInvokes(ExecutionContext context)
        {
        }

        public void Enter(ExecutionContext context,
                          StateChart statechart,
                          OrderedSet<_State> statesForDefaultEntry,
                          SCG.Dictionary<string, OrderedSet<Content>> defaultHistoryContent)
        {
            context.Configuration.Add(this);

            context.StatesToInvoke.Add(this);

            if (!statechart.IsEarlyBinding && _firstEntry)
            {
                context.ExecutionState.Init(statechart, this);

                _firstEntry = false;
            }

            _onEntry.Value.Execute(context, statechart);

            if (statesForDefaultEntry.IsMember(this))
            {
                var transition = this.GetInitialStateTransition();

                transition.ExecuteContent(context, statechart);
            }

            if (defaultHistoryContent.TryGetValue(this.Id, out OrderedSet<Content> set))
            {
                foreach (var content in set.ToList())
                {
                    content.Execute(context, statechart);
                }
            }

            if (this.IsFinalState)
            {
                var parent = this.GetParent(statechart);

                if (parent.IsScxmlRoot)
                {
                    context.IsRunning = false;
                }
                else
                {
                    context.InternalQueue.Enqueue(new Event("done.state." + this.Id, EventType.Internal));

                    var grandparent = parent?.GetParent(statechart);

                    if (grandparent != null && grandparent.IsParallelState)
                    {
                        var parallelChildren = grandparent.GetChildStates(statechart);

                        if (parallelChildren.Every(s => s.IsInFinalState(context, statechart)))
                        {
                            context.InternalQueue.Enqueue(new Event("done.state." + grandparent.Id, EventType.Internal));
                        }
                    }
                }
            }
        }

        public void Exit(ExecutionContext context, StateChart statechart)
        {
            _onExit.Value.Execute(context, statechart);

            CancelOutstandingInvokes(context);

            context.Configuration.Delete(this);
        }

        public void RecordHistory(ExecutionContext context, StateChart statechart)
        {
            foreach (var history in _history.Value)
            {
                Func<_State, bool> predicate;

                if (history.IsDeepHistoryState)
                {
                    predicate = s => s.IsAtomic && s.IsDescendent(this);
                }
                else
                {
                    predicate = s =>
                    {
                        var parent = s.GetParent(statechart);

                        return string.Compare(parent.Id, this.Id, StringComparison.InvariantCultureIgnoreCase) == 0;
                    };
                }

                context.HistoryValue[history.Id] = context.Configuration.ToList().Filter(predicate);
            }
        }

        public _State GetParent(StateChart statechart)
        {
            if (IsScxmlRoot)
            {
                return null;
            }
            else
            {
                return statechart.GetState(_element.Parent);
            }
        }

        public List<_State> GetChildStates(StateChart statechart)
        {
            var states = new List<_State>();

            foreach (var node in _element.Elements().Where(StateChart.IsStateElement))
            {
                states.Append(statechart.GetState(node));
            }

            return states;
        }

        public bool IsInFinalState(ExecutionContext context, StateChart statechart)
        {
            var childStates = GetChildStates(statechart);

            if (_element.Name == "state" && childStates.Some(s => s.IsFinalState && context.Configuration.IsMember(s)))
            {
                return true;
            }
            else if (this.IsParallelState && childStates.Every(s => s.IsInFinalState(context, statechart)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Transition GetInitialStateTransition()
        {
            var attr = _element.Attribute("initial");

            if (attr != null)
            {
                return new Transition(attr.Value, this);
            }
            else
            {
                var initialElement = _element.Element("initial");

                if (initialElement != null)
                {
                    var transitionElement = initialElement.Element("transition");

                    return new Transition(transitionElement, this);
                }
                else if (IsScxmlRoot || IsCompoundState || IsParallelState)
                {
                    var firstChildState = _element.Elements().First(StateChart.IsStateElement);

                    var id = firstChildState.Attribute("id").Value;

                    return new Transition(id, this);
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsDescendent(_State state)
        {
            return state._element.Descendants().Contains(this._element);
        }

        public List<_State> GetProperAncestors(StateChart statechart, _State state = null)
        {
            var set = new List<_State>();

            Func<_State, bool> predicate = _ => true;

            if (state != null)
            {
                if (this.Id == state.Id || state.IsDescendent(this))
                {
                    return set;
                }

                predicate = s => s.Id != state.Id;
            }

            var parent = this.GetParent(statechart);

            while (parent != null && predicate(parent))
            {
                set.Append(parent);

                parent = parent.GetParent(statechart);
            }

            return set;
        }
    }
}
