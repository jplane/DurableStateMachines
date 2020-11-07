using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.DataManipulation;
using CoreEngine.Model.Execution;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CoreEngine.Model.States
{
    internal class Transition
    {
        private readonly XObject _xobj;
        private readonly Lazy<List<ExecutableContent>> _content;
        private readonly string _target;
        private readonly string _events;
        private readonly string _conditionExpr;
        private readonly TransitionType _type;
        private readonly State _source;

        public Transition(XAttribute attribute, State source)
        {
            attribute.CheckArgNull(nameof(attribute));
            source.CheckArgNull(nameof(source));

            _xobj = attribute;
            _content = new Lazy<List<ExecutableContent>>();
            _target = attribute.Value;
            _events = string.Empty;
            _conditionExpr = string.Empty;
            _type = TransitionType.External;
            _source = source;
        }

        public Transition(XElement element, State source)
        {
            element.CheckArgNull(nameof(element));
            source.CheckArgNull(nameof(source));

            _xobj = element;

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

        public static XObject GetXObject(Transition transition)
        {
            transition.CheckArgNull(nameof(transition));

            return transition._xobj;
        }

        public void StoreDefaultHistoryContent(string id, Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            defaultHistoryContent[id] = new Set<ExecutableContent>(_content.Value);
        }

        public bool HasEvent => !string.IsNullOrWhiteSpace(_events);

        public bool HasTargets => !string.IsNullOrWhiteSpace(_target);

        public bool MatchesEvent(Event evt)
        {
            evt.CheckArgNull(nameof(evt));

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

        public async Task<bool> EvaluateCondition(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            async Task<bool> Eval(string condition)
            {
                try
                {
                    return await context.Eval<bool>(condition);
                }
                catch(Exception ex)
                {
                    context.EnqueueExecutionError(ex);
                    return false;
                }
            }

            return string.IsNullOrWhiteSpace(_conditionExpr) ? true : await Eval(_conditionExpr);
        }

        public async Task ExecuteContent(ExecutionContext context)
        {
            foreach (var content in _content.Value)
            {
                await content.Execute(context);
            }
        }

        public bool IsSourceDescendent(Transition transition)
        {
            transition.CheckArgNull(nameof(transition));

            return _source.IsDescendent(transition._source);
        }

        public IEnumerable<State> GetTargetStates(RootState root)
        {
            root.CheckArgNull(nameof(root));

            if (string.IsNullOrWhiteSpace(_target))
            {
                return Enumerable.Empty<State>();
            }
            else
            {
                return _target.Split(" ").Select(id => root.GetState(id));
            }
        }

        public Set<State> GetEffectiveTargetStates(ExecutionContext context, RootState root)
        {
            context.CheckArgNull(nameof(context));

            var targets = new Set<State>();

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
                     targetStates.All(s => s.IsDescendent(_source)))
            {
                return _source;
            }
            else
            {
                var ancestors = _source.GetProperAncestors()
                                       .Where(s => s.IsSequentialState || s.IsScxmlRoot);

                foreach (var ancestor in ancestors)
                {
                    if (targetStates.All(s => s.IsDescendent(ancestor)))
                    {
                        return ancestor;
                    }
                }

                Debug.Assert(_source.IsScxmlRoot);

                return root;
            }
        }
    }

    internal enum TransitionType
    {
        Internal,
        External
    }
}
