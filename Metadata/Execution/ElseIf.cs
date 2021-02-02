using Newtonsoft.Json;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.Execution
{
    /// <summary>
    /// A conditional 'elseif' branch of an if-elseif-else control flow block.
    /// Only used in conjuction with <see cref="If{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "ElseIf",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class ElseIf<TData> : ExecutableContent<TData>, IElseIfMetadata
    {
        private readonly Lazy<Func<dynamic, bool>> _condition;

        private MetadataList<ExecutableContent<TData>> _actions;

        public ElseIf()
        {
            this.Actions = new MetadataList<ExecutableContent<TData>>();

            _condition = new Lazy<Func<dynamic, bool>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ConditionExpression))
                {
                    return ExpressionCompiler.Compile<bool>(this.ConditionExpression);
                }
                else if (this.ConditionFunction != null)
                {
                    return data => this.ConditionFunction((TData) data);
                }
                else
                {
                    return _ => throw new InvalidOperationException("Unable to resolve 'elseif' condition.");
                }
            });
        }

        /// <summary>
        /// Condition evaluated to determine if this <see cref="ElseIf{TData}"/> branch is executed.
        /// Execution state <typeparamref name="TData"/> can be used as part of the conditional logic.
        /// </summary>
        [JsonIgnore]
        public Func<TData, bool> ConditionFunction { get; set; }

        [JsonProperty("conditionexpression", Required = Required.Always)]
        private string ConditionExpression { get; set; }

        /// <summary>
        /// The set of actions executed for this <see cref="ElseIf{TData}"/> branch.
        /// </summary>
        [JsonProperty("actions", ItemConverterType = typeof(ExecutableContentConverter), Required = Required.Always)]
        public MetadataList<ExecutableContent<TData>> Actions
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "elseif"}.actions";

                _actions = value;
            }
        }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.ConditionExpression) && this.ConditionFunction == null)
            {
                errors.Add("One of ConditionExpression or ConditionFunction must be set.");
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

        bool IElseIfMetadata.EvalCondition(dynamic data) => _condition.Value(data);

        IEnumerable<IExecutableContentMetadata> IElseIfMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();
    }
}
