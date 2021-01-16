using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StateChartsDotNet;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DurableFunctionHost
{
    internal class StateMachineContext : ExecutionContextBase
    {
        private readonly IDurableOrchestrationContext _orchestrationContext;

        public StateMachineContext(IStateChartMetadata metadata,
                                   IDurableOrchestrationContext orchestrationContext,
                                   IReadOnlyDictionary<string, object> data,
                                   ILogger logger)
            : base(metadata, default, logger)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));

            _orchestrationContext = orchestrationContext;

            foreach (var pair in data)
            {
                _data[pair.Key] = pair.Value;
            }
        }

        public Dictionary<string, object> ResultData => _data.Where(pair => !pair.Key.StartsWith("_"))
                                                             .ToDictionary(p => p.Key, p => p.Value);

        protected override Task<Guid> GenerateGuid()
        {
            return Task.FromResult(_orchestrationContext.NewGuid());
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

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Inline)
            {
                var json = JObject.Parse((await childMachine.ToStringAsync(default)).Item2);

                Debug.Assert(json != null);

                var payload = new StateMachineRequestPayload
                {
                    Arguments = inputs,
                    StateMachineDefinition = json
                };

                var childData = await _orchestrationContext.CallSubOrchestratorAsync<Dictionary<string, object>>("statemachine-orchestration", instanceId, payload);

                Debug.Assert(childData != null);

                if (!string.IsNullOrWhiteSpace(metadata.ResultLocation))
                {
                    _data[metadata.ResultLocation] = (IReadOnlyDictionary<string, object>) childData;
                }
            }
            else
            {
                //TODO: remote child execution
                throw new NotImplementedException();
            }
        }

        protected override bool IsChildStateChart => _data.ContainsKey("_parentInstanceId");

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

            return _orchestrationContext.CallActivityAsync<string>(type, (target, parameters));
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

            var parms = (target, messageName, content, correlationId, parameters);

            return _orchestrationContext.CallActivityAsync(type, parms);
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
