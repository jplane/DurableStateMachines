using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
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
    public class InvokeStateChart<TData> : IInvokeStateChartMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _getData;

        private MetadataList<ExecutableContent<TData>> _actions;

        public InvokeStateChart()
        {
            this.CompletionActions = new MetadataList<ExecutableContent<TData>>();

            _getData = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.DataExpression))
                {
                    return ExpressionCompiler.Compile<object>(this.DataExpression);
                }
                else if (this.DataFunction != null)
                {
                    return data => this.DataFunction(data);
                }
                else
                {
                    return _ => throw new InvalidOperationException("Unable to resolve 'get data' function.");
                }
            });
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

        public Func<TData, object> DataFunction { get; set; }

        [JsonProperty("dataexpression")]
        private string DataExpression { get; set; }

        [JsonProperty("statemachineidentifier")]
        public string StateMachineIdentifier { get; set; }

        [JsonProperty("completionactions")]
        public MetadataList<ExecutableContent<TData>> CompletionActions
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

            if (string.IsNullOrWhiteSpace(this.StateMachineIdentifier))
            {
                errors.Add("State machine identifier is invalid.");
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

        IStateChartMetadata IInvokeStateChartMetadata.GetRoot() => null;

        string IInvokeStateChartMetadata.GetRootIdentifier() => this.StateMachineIdentifier;

        object IInvokeStateChartMetadata.GetData(dynamic data) => _getData.Value(data);
    }
}
