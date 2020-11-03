using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace CoreEngine.Model
{
    internal class Transition
    {
        private readonly string _targetStateIds;
        private readonly string _events;

        public Transition(string targetStateIds, _State source)
        {
            _targetStateIds = targetStateIds;
            _events = string.Empty;
            Content = new List<Content>();
            Source = source;
            IsInternal = false;
        }

        public Transition(XElement element, _State source)
        {
            _targetStateIds = element.Attribute("target")?.Value;

            _events = element.Attribute("event")?.Value;

            Content = new List<Content>();

            foreach (var node in element.Elements())
            {
                Content.Append(new Content(node));
            }

            Source = source;

            IsInternal = element.Attribute("type") != null && element.Attribute("type").Value == "internal";
        }

        public _State Source { get; }

        public bool IsInternal { get; }

        public bool HasEvent => !string.IsNullOrWhiteSpace(_events);

        public bool HasTargets => !string.IsNullOrWhiteSpace(_targetStateIds);

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

        public bool EvaluateCondition(ExecutionContext context, StateChart statechart)
        {
            return true;
        }

        public List<Content> Content { get; }

        public void ExecuteContent(ExecutionContext context, StateChart statechart)
        {
            foreach (var content in Content.ToList())
            {
                content.Execute(context, statechart);
            }
        }

        public SCG.IEnumerable<_State> GetTargetStates(StateChart statechart)
        {
            if (string.IsNullOrWhiteSpace(_targetStateIds))
            {
                return Enumerable.Empty<_State>();
            }
            else
            {
                return _targetStateIds.Split(" ").Select(id => statechart.GetState(id));
            }
        }

        public OrderedSet<_State> GetEffectiveTargetStates(ExecutionContext context, StateChart statechart)
        {
            var targets = new OrderedSet<_State>();

            foreach (var state in GetTargetStates(statechart))
            {
                if (state.IsHistoryState)
                {
                    if (context.HistoryValue.TryGetValue(state.Id, out List<_State> value))
                    {
                        targets.Union(value);
                    }
                    else
                    {
                        targets.Union(state.GetEffectiveTargetStates(context, statechart));
                    }
                }
                else
                {
                    targets.Add(state);
                }
            }

            return targets;
        }

        public _State GetTransitionDomain(ExecutionContext context, StateChart statechart)
        {
            var targetStates = GetEffectiveTargetStates(context, statechart);

            if (targetStates.IsEmpty())
            {
                return null;
            }
            else if (this.IsInternal &&
                     this.Source.IsCompoundState &&
                     targetStates.Every(s => s.IsDescendent(this.Source)))
            {
                return this.Source;
            }
            else
            {
                var ancestors = this.Source.GetProperAncestors(statechart)
                                           .Filter(s => s.IsCompoundState || s.IsScxmlRoot);

                foreach (var ancestor in ancestors)
                {
                    if (targetStates.Every(s => s.IsDescendent(ancestor)))
                    {
                        return ancestor;
                    }
                }

                throw new InvalidOperationException("Should have at least returned root scxml node.");
            }
        }
    }
}
