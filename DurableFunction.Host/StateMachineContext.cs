using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.DurableFunction.Client;
using StateChartsDotNet.Metadata.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunction.Host
{
    internal class StateMachineContext<TData> : ExecutionContextBase<TData>
    {
        private readonly IDurableOrchestrationContext _orchestrationContext;
        private readonly IConfiguration _config;
        private readonly DebuggerInfo _debugInfo;
        private readonly TData _data;

        public StateMachineContext(IStateChartMetadata metadata,
                                   IDurableOrchestrationContext orchestrationContext,
                                   TData data,
                                   DebuggerInfo debugInfo,
                                   IConfiguration config,
                                   ILogger logger)
            : base(metadata, data, default, logger)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));
            config.CheckArgNull(nameof(config));

            _orchestrationContext = orchestrationContext;
            _config = config;
            _debugInfo = debugInfo;
            _data = data;

            SetDataValue("_instanceId", _orchestrationContext.InstanceId);
            SetDataValue("_parentInstanceId", _orchestrationContext.ParentInstanceId);
        }

        public TData ResultData => _data;

        protected override Guid GenerateGuid()
        {
            return _orchestrationContext.NewGuid();
        }

        internal override async Task<TData> InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentStateMetadataId)
        {
            metadata.CheckArgNull(nameof(metadata));
            parentStateMetadataId.CheckArgNull(nameof(parentStateMetadataId));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);
            Debug.Assert(childMachine is StateMachine<TData>); // for now :-)

            var input = (TData) metadata.GetData(this.ExecutionData);

            var payload = new StateMachineRequestPayload<TData>
            {
                Arguments = input,
                StateMachineIdentifier = metadata.GetRootIdentifier(),
                DebugInfo = _debugInfo,
                IsChildStateMachine = true
            };

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Inline)
            {
                return await _orchestrationContext.CallSubOrchestratorAsync<TData>("statemachine-orchestration", payload);
            }
            else
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(metadata.RemoteUri));

                var uri = new Uri(metadata.RemoteUri);

                var content = JsonConvert.SerializeObject(payload);

                Debug.Assert(!string.IsNullOrWhiteSpace(content));

                var response = await _orchestrationContext.CallHttpAsync(HttpMethod.Post, uri, content);

                Debug.Assert(response != null);

                return JsonConvert.DeserializeObject<TData>(response.Content);
            }
        }

        internal override Task DelayAsync(TimeSpan timespan)
        {
            Debug.Assert(timespan > TimeSpan.Zero);

            var expiration = _orchestrationContext.CurrentUtcDateTime.Add(timespan);

            return _orchestrationContext.CreateTimer(expiration, default);
        }

        private string ResolveConfigValue(string value)
        {
            return _config == null ? value : (value.StartsWith("%") && value.EndsWith("%")) ? _config[value[1..^1]] : value;
        }

        private void ResolveConfigValues(JArray json)
        {
            foreach (var value in json.Values())
            {
                if (value.Type == JTokenType.String)
                {
                    value.Replace(JToken.FromObject(ResolveConfigValue(value.Value<string>())));
                }
                else if (value.Type == JTokenType.Object)
                {
                    ResolveConfigValues(value.Value<JObject>());
                }
                else if (value.Type == JTokenType.Array)
                {
                    ResolveConfigValues(value.Value<JArray>());
                }
            }
        }

        private void ResolveConfigValues(JObject json)
        {
            foreach (var prop in json.Properties())
            {
                if (prop.Value.Type == JTokenType.String)
                {
                    prop.Value = JToken.FromObject(ResolveConfigValue(prop.Value.Value<string>()));
                }
                else if (prop.Value.Type == JTokenType.Object)
                {
                    ResolveConfigValues(prop.Value.Value<JObject>());
                }
                else if (prop.Value.Type == JTokenType.Array)
                {
                    ResolveConfigValues(prop.Value.Value<JArray>());
                }
            }
        }

        internal override Task<string> QueryAsync(string activityType, IQueryConfiguration config)
        {
            activityType.CheckArgNull(nameof(activityType));
            config.CheckArgNull(nameof(config));

            config.ResolveConfigValues(this.ResolveConfigValue);

            if (string.Compare(activityType, "http-get", true, CultureInfo.InvariantCulture) == 0)
            {
                var http = new HttpService(this.ExecutionData, _orchestrationContext);

                return http.GetAsync(config);
            }
            else
            {
                return _orchestrationContext.CallActivityAsync<string>(activityType, config);
            }
        }

        internal override Task SendMessageAsync(string activityType, string correlationId, ISendMessageConfiguration config)
        {
            activityType.CheckArgNull(nameof(activityType));
            config.CheckArgNull(nameof(config));

            config.ResolveConfigValues(this.ResolveConfigValue);

            if (string.Compare(activityType, "http-post", true, CultureInfo.InvariantCulture) == 0)
            {
                var http = new HttpService(this.ExecutionData, _orchestrationContext);

                return http.PostAsync(correlationId, config);
            }
            else
            {
                return _orchestrationContext.CallActivityAsync(activityType, config);
            }
        }

        protected override Task<ExternalMessage> GetNextExternalMessageAsync()
        {
            return _orchestrationContext.WaitForExternalEvent<ExternalMessage>("state-machine-event");
        }

        internal override Task LogDebugAsync(string message)
        {
            _logger.LogDebug(message);

            return Task.CompletedTask;
        }

        internal override Task LogInformationAsync(string message)
        {
            _logger.LogInformation(message);

            return Task.CompletedTask;
        }

        internal override Task BreakOnDebugger(DebuggerAction action, IModelMetadata metadata)
        {
            Debug.Assert(metadata != null);

            if (_debugInfo != null && _debugInfo.IsMatch(action, metadata.MetadataId))
            {
                var info = new Dictionary<string, object>(metadata.DebuggerInfo);

                info["_debuggeraction"] = action;

                foreach (var pair in this.GetDataValues())
                {
                    info.Add(pair.Key, pair.Value);
                }

                if (string.IsNullOrWhiteSpace(_debugInfo.DebugUri))
                {
                    throw new InvalidOperationException("Debugger uri is invalid.");
                }

                var endpoint = ResolveConfigValue(_debugInfo.DebugUri);

                Debug.Assert(!string.IsNullOrWhiteSpace(endpoint));

                return _orchestrationContext.CallActivityAsync("debugger-break", (endpoint, info));
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
