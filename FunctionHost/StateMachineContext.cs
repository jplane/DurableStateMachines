using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSM.Common;
using DSM.Common.Observability;
using DSM.Common.Messages;
using DSM.Common.Model;
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
using DSM.Common.Model.Actions;

namespace DSM.FunctionHost
{
    internal class StateMachineContext : ExecutionContextBase
    {
        private readonly IDurableOrchestrationContext _orchestrationContext;
        private readonly IConfiguration _config;
        private readonly Instruction[] _observableInstructions;

        public StateMachineContext(IStateMachineMetadata metadata,
                                   IDurableOrchestrationContext orchestrationContext,
                                   object data,
                                   string[] parentInstanceIds,
                                   Instruction[] observableInstructions,
                                   IConfiguration config,
                                   Func<string, IStateMachineMetadata> lookupChild,
                                   ILogger logger)
            : base(metadata, default, lookupChild, parentInstanceIds, logger)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));
            config.CheckArgNull(nameof(config));

            _orchestrationContext = orchestrationContext;
            _config = config;
            _observableInstructions = observableInstructions;
            _data = data;

            this.InstanceId = _orchestrationContext.InstanceId;
        }

        public object GetData() => _data;

        protected override Guid GenerateGuid()
        {
            return _orchestrationContext.NewGuid();
        }

        internal override async Task<object> InvokeChildStateMachine(IInvokeStateMachineMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var childMachine = ResolveChildStateMachine(metadata);

            Debug.Assert(childMachine != null);

            (string endpoint, object payload) details = GetEndpointAndPayload(metadata);

            Debug.Assert(!string.IsNullOrWhiteSpace(details.endpoint));
            Debug.Assert(details.payload != null);

            if (metadata.ExecutionMode == ChildStateMachineExecutionMode.Inline)
            {
                return await _orchestrationContext.CallSubOrchestratorAsync<object>(details.endpoint, details.payload);
            }
            else
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(metadata.RemoteUri));

                var uri = new Uri(metadata.RemoteUri);

                var content = JsonConvert.SerializeObject(details.payload);

                Debug.Assert(!string.IsNullOrWhiteSpace(content));

                var response = await _orchestrationContext.CallHttpAsync(HttpMethod.Post, uri, content);

                Debug.Assert(response != null);

                return JsonConvert.DeserializeObject(response.Content);
            }
        }

        private (string, object) GetEndpointAndPayload(IInvokeStateMachineMetadata metadata)
        {
            Debug.Assert(metadata != null);

            var input = metadata.GetData(this.ExecutionData);

            var isByIdentifier = _lookupChild != null;

            var info = metadata.GetStateMachineInfo();

            if (isByIdentifier)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(info.Item1));

                return (StateMachineExtensions.StateMachineWithNameEndpoint, new StateMachinePayload
                {
                    Input = input,
                    StateMachineIdentifier = info.Item1,
                    Observables = _observableInstructions,
                    ParentInstanceStack = this.InstanceIdStack
                });
            }
            else
            {
                Debug.Assert(info.Item2 != null);
                Debug.Assert(info.Item2 is StateMachine<Dictionary<string, object>>);
                Debug.Assert(input == null || input is JObject);

                if (input == null)
                {
                    input = new JObject();
                }

                return (FunctionProvider.StateMachineWithDefinitionEndpoint, new StateMachineDefinitionPayload
                {
                    Input = ((JObject) input).ToObject<Dictionary<string, object>>(),
                    Definition = (StateMachine<Dictionary<string, object>>) info.Item2,
                    Observables = _observableInstructions,
                    ParentInstanceStack = this.InstanceIdStack
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

        internal override Task<string> QueryAsync(string activityType, (object, JObject) config)
        {
            activityType.CheckArgNull(nameof(activityType));

            Debug.Assert(config.Item1 == null);
            Debug.Assert(config.Item2 != null);

            if (string.Compare(activityType, "http-get", true, CultureInfo.InvariantCulture) == 0)
            {
                var http = new HttpService(this.ExecutionData, _orchestrationContext);

                var httpConfig = config.Item2.ToObject<HttpQueryConfiguration>();

                Debug.Assert(httpConfig != null);

                httpConfig.ResolveConfigValues(this.ResolveConfigValue);

                return http.GetAsync(httpConfig);
            }
            else
            {
                ResolveConfigValues(config.Item2);

                return _orchestrationContext.CallActivityAsync<string>(activityType, config.Item2);
            }
        }

        internal override Task SendMessageAsync(string activityType, string correlationId, (object, JObject) config)
        {
            activityType.CheckArgNull(nameof(activityType));

            Debug.Assert(config.Item1 == null);
            Debug.Assert(config.Item2 != null);

            if (string.Compare(activityType, "http-post", true, CultureInfo.InvariantCulture) == 0)
            {
                var httpConfig = config.Item2.ToObject<HttpSendMessageConfiguration>();

                Debug.Assert(httpConfig != null);

                httpConfig.ResolveConfigValues(this.ResolveConfigValue);

                var http = new HttpService(this.ExecutionData, _orchestrationContext);

                return http.PostAsync(correlationId, httpConfig);
            }
            else
            {
                ResolveConfigValues(config.Item2);

                return _orchestrationContext.CallActivityAsync(activityType, config.Item2);
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

        internal override Task OnAction(ObservableAction action, IModelMetadata metadata)
        {
            Debug.Assert(metadata != null);

            if (_observableInstructions != null && _observableInstructions.IsMatch(action, metadata.MetadataId))
            {
                var info = new Dictionary<string, object>(metadata.DebuggerInfo);

                info["_action"] = action;

                foreach (var pair in this.GetDebuggerValues())
                {
                    info.Add(pair.Key, pair.Value);
                }

                var endpoint = _config["DEBUGGER_URI"];

                Debug.Assert(!string.IsNullOrWhiteSpace(endpoint));

                var input = (endpoint, info, this.InstanceIdStack);

                return _orchestrationContext.CallActivityAsync(FunctionProvider.StateMachineDebuggerBreakEndpoint, input);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
