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
using StateChartsDotNet.Common.Debugger;

namespace StateChartsDotNet.Model.States
{
    internal class Transition<TData>
    {
        private readonly Lazy<ExecutableContent<TData>[]> _content;
        private readonly ITransitionMetadata _metadata;
        private readonly State<TData> _source;

        public Transition(ITransitionMetadata metadata, State<TData> source)
        {
            metadata.CheckArgNull(nameof(metadata));
            source.CheckArgNull(nameof(source));

            _metadata = metadata;
            _source = source;

            _content = new Lazy<ExecutableContent<TData>[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent<TData>.Create).ToArray();
            });
        }

        public void StoreDefaultHistoryContent(string id, Dictionary<string, Set<ExecutableContent<TData>>> defaultHistoryContent)
        {
            defaultHistoryContent.CheckArgNull(nameof(defaultHistoryContent));

            defaultHistoryContent[id] = new Set<ExecutableContent<TData>>(_content.Value);
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

        public bool EvaluateCondition(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            try
            {
                return _metadata.EvalCondition(context.ExecutionData);
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);

                return false;
            }
        }

        public async Task ExecuteContentAsync(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            await context.BreakOnDebugger(DebuggerAction.MakeTransition, _metadata);

            if (_metadata.Delay != null)
            {
                await context.DelayAsync(_metadata.Delay.Value);
            }

            foreach (var content in _content.Value)
            {
                await content.ExecuteAsync(context);
            }
        }

        public bool IsSourceDescendent(Transition<TData> transition)
        {
            transition.CheckArgNull(nameof(transition));

            return _source.IsDescendent(transition._source);
        }

        public IEnumerable<State<TData>> GetTargetStates(StateChart<TData> root)
        {
            root.CheckArgNull(nameof(root));

            var targets = new List<State<TData>>();

            foreach (var id in _metadata.Targets)
            {
                targets.Add(root.GetState(id));
            }

            return targets.AsEnumerable();
        }

        public Set<State<TData>> GetEffectiveTargetStates(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            var targets = new Set<State<TData>>();

            foreach (var state in GetTargetStates(context.Root))
            {
                Debug.Assert(state != null);

                if (state.Type == StateType.History)
                {
                    if (context.TryGetHistoryValue(state.Id, out IEnumerable<State<TData>> value))
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

        public State<TData> GetTransitionDomain(ExecutionContextBase<TData> context)
        {
            var targetStates = GetEffectiveTargetStates(context);

            if (targetStates.IsEmpty())
            {
                return null;
            }
            else if (_metadata.Type == TransitionType.Internal &&
                     _source.Type == StateType.Compound &&
                     targetStates.All(s => s.IsDescendent(_source)))
            {
                return _source;
            }
            else
            {
                var ancestors = _source.GetProperAncestors()
                                       .Where(s => s.Type == StateType.Compound || s.Type == StateType.Root);

                foreach (var ancestor in ancestors)
                {
                    if (targetStates.All(s => s.IsDescendent(ancestor)))
                    {
                        return ancestor;
                    }
                }

                Debug.Assert(_source.Type == StateType.Root);

                return context.Root;
            }
        }
    }
}
