using System;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;

namespace StateChartsDotNet.Model.States
{
    internal class Transition
    {
        private readonly Lazy<ExecutableContent[]> _content;
        private readonly ITransitionMetadata _metadata;
        private readonly State _source;

        public Transition(ITransitionMetadata metadata, State source)
        {
            metadata.CheckArgNull(nameof(metadata));
            source.CheckArgNull(nameof(source));

            _metadata = metadata;
            _source = source;

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public void StoreDefaultHistoryContent(string id, Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            defaultHistoryContent[id] = new Set<ExecutableContent>(_content.Value);
        }

        public bool HasMessage => _metadata.Messages.Any();

        public bool HasTargets => _metadata.Targets.Any();

        public bool MatchesMessage(Message evt)
        {
            evt.CheckArgNull(nameof(evt));

            if (HasMessage)
            {
                foreach (var candidateEvt in _metadata.Messages)
                {
                    if (candidateEvt == "*")
                    {
                        return true;
                    }
                    else if (evt.Name.ToLowerInvariant().StartsWith(candidateEvt.TrimEnd('*', '.').ToLowerInvariant()))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public bool EvaluateCondition(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            try
            {
                return _metadata.EvalCondition(context.ScriptData);
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);

                return false;
            }
        }

        public async Task ExecuteContentAsync(ExecutionContextBase context)
        {
            foreach (var content in _content.Value)
            {
                await content.ExecuteAsync(context);
            }
        }

        public bool IsSourceDescendent(Transition transition)
        {
            transition.CheckArgNull(nameof(transition));

            return _source.IsDescendent(transition._source);
        }

        public IEnumerable<State> GetTargetStates(StartChart root)
        {
            root.CheckArgNull(nameof(root));

            var targets = new List<State>();

            foreach (var id in _metadata.Targets)
            {
                targets.Add(root.GetState(id));
            }

            return targets.AsEnumerable();
        }

        public Set<State> GetEffectiveTargetStates(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var targets = new Set<State>();

            foreach (var state in GetTargetStates(context.Root))
            {
                if (state.IsHistoryState)
                {
                    if (context.TryGetHistoryValue(state.Id, out IEnumerable<State> value))
                    {
                        targets.Union(value);
                    }
                    else
                    {
                        targets.Union(state.GetEffectiveTargetStates(context));
                    }
                }
                else
                {
                    targets.Add(state);
                }
            }

            return targets;
        }

        public State GetTransitionDomain(ExecutionContextBase context)
        {
            var targetStates = GetEffectiveTargetStates(context);

            if (targetStates.IsEmpty())
            {
                return null;
            }
            else if (_metadata.Type == TransitionType.Internal &&
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

                return context.Root;
            }
        }
    }
}
