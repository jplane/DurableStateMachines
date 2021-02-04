using Newtonsoft.Json;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using DSM.Common.Model.States;
using DSM.Metadata.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.States
{
    /// <summary>
    /// <see cref="Transition{TData}"/> maps a set of conditions (changes to execution state <see cref="TData"/>) or arrival
    ///  of events to zero or more target <see cref="State{TData}"/>s and an optional set of transition <see cref="Actions"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "Transition",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class Transition<TData> : ITransitionMetadata
    {
        private readonly Lazy<Func<dynamic, bool>> _condition;

        private MetadataList<ExecutableContent<TData>> _actions;
        private string _syntheticMetadataId;

        public Transition()
        {
            this.Actions = new MetadataList<ExecutableContent<TData>>();

            _condition = new Lazy<Func<dynamic, bool>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ConditionExpression))
                {
                    return ExpressionCompiler.Compile<bool>(((IModelMetadata)this).MetadataId, this.ConditionExpression);
                }
                else if (this.ConditionFunction != null)
                {
                    return data => this.ConditionFunction((TData) data);
                }
                else
                {
                    return _ => true;
                }
            });
        }

        public Transition(string target, string parentMetadataId)
        {
            this.Target = target;

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

        /// <summary>
        /// Target <see cref="State{TData}"/> name for this transition. If empty, a matched <see cref="Transition{TData}"/> will
        ///  execute its configured actions but will not result in a state transition.
        /// To target multiple child states in a <see cref="ParallelState{TData}"/>, specify multiple state names separated by commas.
        /// </summary>
        [JsonProperty("target", Required = Required.DisallowNull)]
        public string Target { get; set; }

        /// <summary>
        /// Triggering event message name for this transition. Specify multiple names by separating with commas. Can be empty. Can be combined with
        ///  <see cref="ConditionFunction"/> to define transition logic.
        ///  Events are raised both internally by state machine execution, by a <see cref="Raise{TData}"/> action, or from external sources
        ///  using the Durable Functions 'external events' feature: https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-external-events?tabs=csharp
        /// </summary>
        [JsonProperty("message", Required = Required.DisallowNull)]
        public string Message { get; set; }

        /// <summary>
        /// An optional delay for this <see cref="Transition{TData}"/> to its target <see cref="State{TData}"/>. If null, the transition is immediate.
        /// </summary>
        [JsonProperty("delay", Required = Required.DisallowNull)]
        public TimeSpan? Delay { get; set; }

        /// <summary>
        /// Affects whether a parent <see cref="CompoundState{TData}"/> is exited if this <see cref="Transition{TData}"/> targets a child of that same parent.
        /// <see cref="TransitionType.External"/> means the parent <see cref="CompoundState{TData}"/> is exited (<see cref="CompoundState{TData}.OnExit"/> fires, etc.)
        /// <see cref="TransitionType.Internal"/> means the parent <see cref="CompoundState{TData}"/> is not exited.
        /// </summary>
        [JsonProperty("type", Required = Required.DisallowNull)]
        public TransitionType Type { get; set; }

        /// <summary>
        /// Condition evaluated to determine if this <see cref="Transition{TData}{TData}"/> is triggered. Can be null (effectively condition == true) and can
        ///  also be combined with event names using <see cref="Transition{TData}.Message"/>.
        /// </summary>
        [JsonIgnore]
        public Func<TData, bool> ConditionFunction { get; set; }

        [JsonProperty("conditionexpression", Required = Required.DisallowNull)]
        private string ConditionExpression { get; set; }

        /// <summary>
        /// The set of actions executed for this <see cref="Transition{TData}"/>, when triggered.
        /// </summary>
        [JsonProperty("actions", ItemConverterType = typeof(ExecutableContentConverter), Required = Required.DisallowNull)]
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "transition"}.actions";

                _actions = value;
            }
        }

        internal void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Target) && string.IsNullOrWhiteSpace(this.Message))
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

        IEnumerable<string> ITransitionMetadata.Targets =>
            string.IsNullOrWhiteSpace(this.Target) ? Enumerable.Empty<string>() : this.Target.Split(",");

        IEnumerable<string> ITransitionMetadata.Messages =>
            string.IsNullOrWhiteSpace(this.Message) ? Enumerable.Empty<string>() : this.Message.Split(",");

        bool ITransitionMetadata.EvalCondition(dynamic data) => _condition.Value(data);

        IEnumerable<IExecutableContentMetadata> ITransitionMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();
    }
}
