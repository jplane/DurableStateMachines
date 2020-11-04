using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.DataManipulation;
using CoreEngine.Model.Execution;

namespace CoreEngine.Model.States
{
    internal class Transition
    {
        private readonly Lazy<List<ExecutableContent>> _content;
        private readonly string _target;
        private readonly string _events;
        private readonly string _conditionExpr;
        private readonly TransitionType _type;
        private readonly State _source;

        public Transition(string target, State source)
        {
            _content = new Lazy<List<ExecutableContent>>();
            _target = target;
            _events = string.Empty;
            _conditionExpr = string.Empty;
            _type = TransitionType.External;
            _source = source;
        }

        public Transition(XElement element, State source)
        {
            _target = element.Attribute("target")?.Value ?? string.Empty;

            _events = element.Attribute("event")?.Value ?? string.Empty;

            _conditionExpr = element.Attribute("cond")?.Value ?? string.Empty;

            _type = (TransitionType) Enum.Parse(typeof(TransitionType),
                                                element.Attribute("type")?.Value ?? "external",
                                                true);

            _content = new Lazy<List<ExecutableContent>>(() =>
            {
                var content = new List<ExecutableContent>();

                foreach (var node in element.Elements())
                {
                    content.Add(ExecutableContent.Create(node));
                }

                return content;
            });

            _source = source;
        }

        public void StoreDefaultHistoryContent(string id, Dictionary<string, OrderedSet<ExecutableContent>> defaultHistoryContent)
        {
            defaultHistoryContent[id] = OrderedSet<ExecutableContent>.Create(_content.Value);
        }

        public bool HasEvent => !string.IsNullOrWhiteSpace(_events);

        public bool HasTargets => !string.IsNullOrWhiteSpace(_target);

        public bool MatchesEvent(Event evt)
        {
            if (HasEvent)
            {
                return _events.Split(" ").Any(evtId => string.Compare(evtId,
                                                                      evt.Name,
                                                                      StringComparison.InvariantCultureIgnoreCase) == 0);
            }
            else
            {
                return false;
            }
        }

        public bool EvaluateCondition(ExecutionContext context, RootState root)
        {
            return true;
        }

        public void ExecuteContent(ExecutionContext context)
        {
            foreach (var content in _content.Value.ToList())
            {
                content.Execute(context);
            }
        }

        public bool IsSourceDescendent(Transition transition)
        {
            return _source.IsDescendent(transition._source);
        }

        public IEnumerable<State> GetTargetStates(RootState root)
        {
            if (string.IsNullOrWhiteSpace(_target))
            {
                return Enumerable.Empty<State>();
            }
            else
            {
                return _target.Split(" ").Select(id => root.GetState(id));
            }
        }

        public OrderedSet<State> GetEffectiveTargetStates(ExecutionContext context, RootState root)
        {
            var targets = new OrderedSet<State>();

            foreach (var state in GetTargetStates(root))
            {
                if (state.IsHistoryState)
                {
                    if (context.TryGetHistoryValue(state.Id, out IEnumerable<State> value))
                    {
                        targets.Union(value);
                    }
                    else
                    {
                        targets.Union(state.GetEffectiveTargetStates(context, root));
                    }
                }
                else
                {
                    targets.Add(state);
                }
            }

            return targets;
        }

        public State GetTransitionDomain(ExecutionContext context, RootState root)
        {
            var targetStates = GetEffectiveTargetStates(context, root);

            if (targetStates.IsEmpty())
            {
                return null;
            }
            else if (_type == TransitionType.Internal &&
                     _source.IsSequentialState &&
                     targetStates.Every(s => s.IsDescendent(_source)))
            {
                return _source;
            }
            else
            {
                var ancestors = _source.GetProperAncestors()
                                       .Where(s => s.IsSequentialState || s.IsScxmlRoot);

                foreach (var ancestor in ancestors)
                {
                    if (targetStates.Every(s => s.IsDescendent(ancestor)))
                    {
                        return ancestor;
                    }
                }

                throw new InvalidOperationException("Should have returned sequential or root scxml node.");
            }
        }
    }

    internal enum TransitionType
    {
        Internal,
        External
    }
}
