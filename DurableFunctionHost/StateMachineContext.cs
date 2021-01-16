using DurableFunctionHost;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunctionHost
{
    internal class StateMachineContext : ExecutionContextBase
    {
        private readonly IDurableOrchestrationContext _orchestrationContext;
        private readonly IConfiguration _config;

        public StateMachineContext(IStateChartMetadata metadata,
                                   IDurableOrchestrationContext orchestrationContext,
                                   IReadOnlyDictionary<string, object> data,
                                   IConfiguration config,
                                   ILogger logger)
            : base(metadata, default, logger)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));
            config.CheckArgNull(nameof(config));

            _orchestrationContext = orchestrationContext;
            _config = config;

            foreach (var pair in data)
            {
                SetDataValue(pair.Key, pair.Value);
            }
        }

        public Dictionary<string, object> ResultData => this.GetDataValues().Where(pair => !pair.Key.StartsWith("_"))
                                                                            .ToDictionary(p => p.Key, p => p.Value);

        protected override Task<Guid> GenerateGuid()
        {
            return Task.FromResult(_orchestrationContext.NewGuid());
        }

        public override string ResolveConfigValue(string value)
        {
            return _config == null ? value : (value.StartsWith("%") && value.EndsWith("%")) ? _config[value[1..^1]] : value;
        }

        internal override async Task InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentStateMetadataId)
        {
            metadata.CheckArgNull(nameof(metadata));
            parentStateMetadataId.CheckArgNull(nameof(parentStateMetadataId));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            var inputs = new Dictionary<string, object>(metadata.GetParams(this.ScriptData)
                                                                .ToDictionary(p => p.Key, p => p.Value));

            inputs["_parentInstanceId"] = _orchestrationContext.InstanceId;

            var instanceId = (await GenerateGuid()).ToString("N");

            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            inputs["_instanceId"] = instanceId;

            var json = JObject.Parse((await childMachine.ToStringAsync(default)).Item2);

            Debug.Assert(json != null);

            var payload = new StateMachineRequestPayload
            {
                Arguments = inputs,
                StateMachineDefinition = json
            };

            Dictionary<string, object> childData = null;

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Inline)
            {
                childData = await _orchestrationContext.CallSubOrchestratorAsync<Dictionary<string, object>>("statemachine-orchestration", instanceId, payload);
            }
            else
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(metadata.RemoteUri));

                var uri = new Uri(metadata.RemoteUri);

                var content = JsonConvert.SerializeObject(payload);

                Debug.Assert(!string.IsNullOrWhiteSpace(content));

                var response = await _orchestrationContext.CallHttpAsync(HttpMethod.Post, uri, content);

                Debug.Assert(response != null);

                childData = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
            }

            Debug.Assert(childData != null);

            if (!string.IsNullOrWhiteSpace(metadata.ResultLocation))
            {
                SetDataValue(metadata.ResultLocation, (IReadOnlyDictionary<string, object>) childData);
            }
        }

        protected override bool IsChildStateChart => this.GetDataValues().Any(pair => pair.Key == "_parentInstanceId");

        internal override Task DelayAsync(TimeSpan timespan)
        {
            Debug.Assert(timespan > TimeSpan.Zero);

            var expiration = _orchestrationContext.CurrentUtcDateTime.Add(timespan);

            return _orchestrationContext.CreateTimer(expiration, default);
        }

        internal override Task<string> QueryAsync(string type, string target, IReadOnlyDictionary<string, object> parameters)
        {
            type.CheckArgNull(nameof(type));
            target.CheckArgNull(nameof(target));
            parameters.CheckArgNull(nameof(parameters));

            if (string.Compare(type, "http-get", true, CultureInfo.InvariantCulture) == 0)
            {
                var http = new HttpService(_orchestrationContext);

                return http.GetAsync(target, parameters);
            }
            else
            {
                return _orchestrationContext.CallActivityAsync<string>(type, (target, parameters));
            }
        }

        internal override Task SendMessageAsync(string type,
                                                string target,
                                                string messageName,
                                                object content,
                                                string correlationId,
                                                IReadOnlyDictionary<string, object> parameters)
        {
            type.CheckArgNull(nameof(type));
            target.CheckArgNull(nameof(target));
            parameters.CheckArgNull(nameof(parameters));

            if (string.Compare(type, "http-post", true, CultureInfo.InvariantCulture) == 0)
            {
                var http = new HttpService(_orchestrationContext);

                return http.PostAsync(target, null, content, correlationId, parameters);
            }
            else if (string.Compare(type, "http-put", true, CultureInfo.InvariantCulture) == 0)
            {
                var http = new HttpService(_orchestrationContext);

                return http.PutAsync(target, null, content, correlationId, parameters);
            }
            else
            {
                var parms = (target, messageName, content, correlationId, parameters);

                return _orchestrationContext.CallActivityAsync(type, parms);
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
    }
}
