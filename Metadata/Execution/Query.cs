using Newtonsoft.Json;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace DSM.Metadata.Execution
{
    /// <summary>
    /// An action that models a request-response operation as a Durable Functions Activity invocation.
    /// To use, implement <see cref="IQueryConfiguration"/> and an activity that accepts an instance of your custom type as input.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "Query",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class Query<TData> : ExecutableContent<TData>, IQueryMetadata
    {
        private MemberInfo _resultTarget;
        private MetadataList<ExecutableContent<TData>> _actions;

        public Query()
        {
            this.Actions = new MetadataList<ExecutableContent<TData>>();
        }

        /// <summary>
        /// Name of the custom activity that models your request-response operation.
        /// </summary>
        [JsonProperty("activitytype", Required = Required.Always)]
        public string ActivityType { get; set; }

        /// <summary>
        /// Target field or property in <typeparamref name="TData"/> into which the query response should be assigned.
        /// </summary>
        [JsonIgnore]
        public Expression<Func<TData, object>> AssignTo
        {
            set => _resultTarget = value.ExtractMember(nameof(AssignTo));
        }

        [JsonProperty("resultlocation", Required = Required.Always)]
        private string ResultLocation { get; set; }

        /// <summary>
        /// An instance of your custom configuration that provides all needed information for the query operation.
        /// Instances of this class should be JSON-serializable.
        /// </summary>
        public object Configuration { get; set; }

        [JsonProperty("configuration", Required = Required.Always)]
        private JObject JsonConfig { get; set; }

        /// <summary>
        /// The set of actions executed for this <see cref="Query{TData}"/> once a successful response is received.
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "query"}.actions";

                _actions = value;
            }
        }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (this.Configuration == null && this.JsonConfig == null)
            {
                errors.Add("Configuration is invalid.");
            }

            if (string.IsNullOrWhiteSpace(this.ActivityType))
            {
                errors.Add("ActivityType is invalid.");
            }

            if (string.IsNullOrWhiteSpace(this.ResultLocation) && _resultTarget == null)
            {
                errors.Add("Result target/location is invalid.");
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

        (string, MemberInfo) IQueryMetadata.ResultLocation => (this.ResultLocation, _resultTarget);

        IEnumerable<IExecutableContentMetadata> IQueryMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();

        (object, JObject) IQueryMetadata.GetConfiguration() => (this.Configuration, this.JsonConfig);
    }
}
