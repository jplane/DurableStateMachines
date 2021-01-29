using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.ExpressionTrees;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.States
{
    public class Transition : ITransitionMetadata
    {
        private readonly Lazy<Func<dynamic, bool>> _condition;

        private MetadataList<ExecutableContent> _actions;
        private string _syntheticMetadataId;

        public Transition()
        {
            this.Actions = new MetadataList<ExecutableContent>();
            this.Targets = new List<string>();
            this.Messages = new List<string>();

            _condition = new Lazy<Func<dynamic, bool>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ConditionExpression))
                {
                    return ExpressionCompiler.Compile<bool>(this.ConditionExpression);
                }
                else if (this.ConditionFunction != null)
                {
                    var func = this.ConditionFunction.Compile();

                    Debug.Assert(func != null);

                    return data => func((IDictionary<string, object>)data);
                }
                else
                {
                    return _ => true;
                }
            });
        }

        public Transition(string target, string parentMetadataId)
        {
            this.Targets = new List<string> { target };

            _condition = new Lazy<Func<dynamic, bool>>(() => _ => true);

            _syntheticMetadataId = $"{parentMetadataId}.initialTransition";
        }

        internal Func<IModelMetadata, string> MetadataIdResolver { private get; set; }

        string IModelMetadata.MetadataId =>
            string.IsNullOrWhiteSpace(_syntheticMetadataId) ? this.MetadataIdResolver?.Invoke(this) : _syntheticMetadataId;

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["metadataId"] = ((IModelMetadata)this).MetadataId;

                return info;
            }
        }

        [JsonProperty("targets")]
        public List<string> Targets { get; set; }

        [JsonProperty("messages")]
        public List<string> Messages { get; set; }

        [JsonProperty("delay")]
        public TimeSpan? Delay { get; set; }

        [JsonProperty("type")]
        public TransitionType Type { get; set; }

        [JsonProperty("conditionfunction", ItemConverterType = typeof(ExpressionTreeConverter))]
        public Expression<Func<IDictionary<string, object>, bool>> ConditionFunction { get; set; }

        [JsonProperty("conditionexpression")]
        public string ConditionExpression { get; set; }

        [JsonProperty("actions", ItemConverterType = typeof(ExecutableContentConverter))]
        public MetadataList<ExecutableContent> Actions
        {
            get => _actions;

            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_actions != null)
                {
                    _actions.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "transition"}.actions";

                _actions = value;
            }
        }

        internal void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (this.Targets.Count == 0 && this.Messages.Count == 0)
            {
                errors.Add("Transition must specify at least one target state or one matching message.");
            }

            foreach (var action in this.Actions)
            {
                action.Validate(errorMap);
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        IEnumerable<string> ITransitionMetadata.Targets => this.Targets ?? Enumerable.Empty<string>();

        IEnumerable<string> ITransitionMetadata.Messages => this.Messages ?? Enumerable.Empty<string>();

        bool ITransitionMetadata.EvalCondition(dynamic data) => _condition.Value(data);

        IEnumerable<IExecutableContentMetadata> ITransitionMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();
    }
}
