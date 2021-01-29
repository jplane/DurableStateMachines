using Newtonsoft.Json;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateChartsDotNet.Metadata.States
{
    public class InvokeStateChart : IInvokeStateChartMetadata
    {
        private MetadataList<ExecutableContent> _actions;
        private StateMachine _definition;

        public InvokeStateChart()
        {
            this.CompletionActions = new MetadataList<ExecutableContent>();
            this.Parameters = new List<Param>();
        }

        internal Func<IModelMetadata, string> MetadataIdResolver { private get; set; }

        string IModelMetadata.MetadataId => this.MetadataIdResolver?.Invoke(this);

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resultlocation")]
        public string ResultLocation { get; set; }

        [JsonProperty("mode")]
        public ChildStateChartExecutionMode ExecutionMode { get; set; }

        [JsonProperty("remoteuri")]
        public string RemoteUri { get; set; }

        [JsonProperty("definition")]
        public StateMachine Definition
        {
            get => _definition;
            set => _definition = value;
        }

        [JsonProperty("parameters")]
        public List<Param> Parameters { get; set; }

        [JsonProperty("completionactions", ItemConverterType = typeof(ExecutableContentConverter))]
        public MetadataList<ExecutableContent> CompletionActions
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "invoke"}.actions";

                _actions = value;
            }
        }

        internal void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Id))
            {
                errors.Add("Id is invalid.");
            }

            if (this.ExecutionMode == ChildStateChartExecutionMode.Remote &&
                string.IsNullOrWhiteSpace(this.RemoteUri))
            {
                errors.Add("ChildStateChartExecutionMode.Remote requires a RemoteUri value.");
            }

            if (this.Definition == null)
            {
                errors.Add("Definition is invalid.");
            }

            this.Definition?.Validate(errorMap);

            for (var i = 0; i < this.Parameters.Count; i++)
            {
                var param = this.Parameters[i];

                param.Validate($"{((IModelMetadata) this).MetadataId}.params[{i}]", errorMap);
            }

            foreach (var action in this.CompletionActions)
            {
                action.Validate(errorMap);
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["metadataId"] = ((IModelMetadata) this).MetadataId;

                return info;
            }
        }

        IEnumerable<IExecutableContentMetadata> IInvokeStateChartMetadata.GetFinalizeExecutableContent() =>
            this.CompletionActions ?? Enumerable.Empty<IExecutableContentMetadata>();

        IStateChartMetadata IInvokeStateChartMetadata.GetRoot() => this.Definition;

        IReadOnlyDictionary<string, object> IInvokeStateChartMetadata.GetParams(dynamic data) =>
            this.Parameters?.ToDictionary(p => p.Name, p => p.GetValue(data)) ?? new Dictionary<string, object>();
    }
}
