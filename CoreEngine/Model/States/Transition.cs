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
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.Abstractions.Model;

namespace CoreEngine.Model.States
{
    internal class Transition
    {
        private readonly AsyncLazy<ExecutableContent[]> _content;
        private readonly ITransitionMetadata _metadata;
        private readonly State _source;

        public Transition(ITransitionMetadata metadata, State source)
        {
            metadata.CheckArgNull(nameof(metadata));
            source.CheckArgNull(nameof(source));

            _metadata = metadata;
            _source = source;

            _content = new AsyncLazy<ExecutableContent[]>(async () =>
            {
                return (await metadata.GetExecutableContent()).Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task StoreDefaultHistoryContent(string id, Dictionary<string, Set<ExecutableContent>> defaultHistoryContent)
        {
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            defaultHistoryContent[id] = new Set<ExecutableContent>(await _content);
        }

        public bool HasEvent => _metadata.Events.Any();

        public bool HasTargets => _metadata.Targets.Any();

        public bool MatchesEvent(Event evt)
        {
            evt.CheckArgNull(nameof(evt));

            if (HasEvent)
            {
                foreach (var candidateEvt in _metadata.Events)
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

            return string.IsNullOrWhiteSpace(_metadata.ConditionExpr) ? true : await Eval(_metadata.ConditionExpr);
        }

        public async Task ExecuteContent(ExecutionContext context)
        {
            foreach (var content in await _content)
            {
                await content.Execute(context);
            }
        }

        public bool IsSourceDescendent(Transition transition)
        {
            transition.CheckArgNull(nameof(transition));

            return _source.IsDescendent(transition._source);
        }

        public async Task<IEnumerable<State>> GetTargetStates(RootState root)
        {
            root.CheckArgNull(nameof(root));

            var targets = new List<State>();

            foreach (var id in _metadata.Targets)
            {
                targets.Add(await root.GetState(id));
            }

            return targets.AsEnumerable();
        }

        public async Task<Set<State>> GetEffectiveTargetStates(ExecutionContext context, RootState root)
        {
            context.CheckArgNull(nameof(context));

            var targets = new Set<State>();

            foreach (var state in await GetTargetStates(root))
            {
                if (state.IsHistoryState)
                {
                    if (context.TryGetHistoryValue(state.Id, out IEnumerable<State> value))
                    {
                        targets.Union(value);
                    }
                    else
                    {
                        targets.Union(await state.GetEffectiveTargetStates(context, root));
                    }
                }
                else
                {
                    targets.Add(state);
                }
            }

            return targets;
        }

        public async Task<State> GetTransitionDomain(ExecutionContext context, RootState root)
        {
            var targetStates = await GetEffectiveTargetStates(context, root);

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

                return root;
            }
        }
    }
}
