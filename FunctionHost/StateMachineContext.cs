using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSM.Common;
using DSM.Common.Debugger;
using DSM.Common.Messages;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using DSM.Common.Model.States;
using DSM.Engine;
using DSM.FunctionClient;
using DSM.Metadata.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace DSM.FunctionHost
{
    internal class StateMachineContext : ExecutionContextBase
    {
        private readonly IDurableOrchestrationContext _orchestrationContext;
        private readonly IConfiguration _config;
        private readonly DebuggerInfo _debugInfo;

        public StateMachineContext(IStateChartMetadata metadata,
                                   IDurableOrchestrationContext orchestrationContext,
                                   object data,
                                   bool isChild,
                                   DebuggerInfo debugInfo,
                                   IConfiguration config,
                                   Func<string, IStateChartMetadata> lookupChild,
                                   ILogger logger)
            : base(metadata, default, lookupChild, isChild, logger)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));
            config.CheckArgNull(nameof(config));

            _orchestrationContext = orchestrationContext;
            _config = config;
            _debugInfo = debugInfo;
            _data = data;

            SetInternalDataValue("_instanceId", _orchestrationContext.InstanceId);
            SetInternalDataValue("_parentInstanceId", _orchestrationContext.ParentInstanceId);
        }

        public object GetData() => _data;

        protected override Guid GenerateGuid()
        {
            return _orchestrationContext.NewGuid();
        }

        internal override async Task<object> InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentStateMetadataId)
        {
            metadata.CheckArgNull(nameof(metadata));
            parentStateMetadataId.CheckArgNull(nameof(parentStateMetadataId));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            (string endpoint, object payload) details = GetEndpointAndPayload(metadata);

            Debug.Assert(!string.IsNullOrWhiteSpace(details.endpoint));
            Debug.Assert(details.payload != null);

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Inline)
            {
                return await _orchestrationContext.CallSubOrchestratorAsync<object>(details.endpoint, details.payload);
            }
            else
            {
                Debug.Fail("Need to sort out by-id vs. by-def for remote invocation.");

                Debug.Assert(!string.IsNullOrWhiteSpace(metadata.RemoteUri));

                var uri = new Uri(metadata.RemoteUri);

                var content = JsonConvert.SerializeObject(details.payload);

                Debug.Assert(!string.IsNullOrWhiteSpace(content));

                var response = await _orchestrationContext.CallHttpAsync(HttpMethod.Post, uri, content);

                Debug.Assert(response != null);

                return JsonConvert.DeserializeObject(response.Content);
            }
        }

        private (string, object) GetEndpointAndPayload(IInvokeStateChartMetadata metadata)
        {
            Debug.Assert(metadata != null);

            var input = metadata.GetData(this.ExecutionData);

            var isByIdentifier = _lookupChild != null;

            if (isByIdentifier)
            {
                return (StateMachineExtensions.StateMachineWithNameEndpoint, new StateMachinePayload
                {
                    Input = input,
                    StateMachineIdentifier = metadata.GetRootIdentifier(),
                    DebugInfo = _debugInfo,
                    IsChildStateMachine = true
                });
            }
            else
            {
                return (FunctionProvider.StateMachineWithDefinitionEndpoint, new StateMachineDefinitionPayload
                {
                    Input = (Dictionary<string, object>) input,
                    Definition = (StateMachine<Dictionary<string, object>>) metadata.GetRoot(),
                    DebugInfo = _debugInfo,
                    IsChildStateMachine = true
                });
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

                foreach (var pair in this.GetDebuggerValues())
                {
                    info.Add(pair.Key, pair.Value);
                }

                var endpoint = _config["DEBUGGER_URI"];

                Debug.Assert(!string.IsNullOrWhiteSpace(endpoint));

                return _orchestrationContext.CallActivityAsync("statemachine-debugger-break", (endpoint, info));
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
