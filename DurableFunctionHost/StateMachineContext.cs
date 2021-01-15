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
        private readonly Dictionary<string, List<(string, string)>> _childInstances;
        private readonly IDurableOrchestrationContext _orchestrationContext;
        private readonly IDurableOrchestrationClient _orchestrationClient;

        public StateMachineContext(IStateChartMetadata metadata,
                                   IDurableOrchestrationClient orchestrationClient,
                                   IDurableOrchestrationContext orchestrationContext,
                                   IReadOnlyDictionary<string, object> data,
                                   ILogger logger)
            : base(metadata, default, logger)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationClient.CheckArgNull(nameof(orchestrationClient));
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));

            _orchestrationClient = orchestrationClient;
            _orchestrationContext = orchestrationContext;

            foreach (var pair in data)
            {
                _data[pair.Key] = pair.Value;
            }

            _childInstances = new Dictionary<string, List<(string, string)>>();
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

            if (!string.IsNullOrWhiteSpace(metadata.IdLocation))
            {
                _data[metadata.IdLocation] = instanceId;
            }

            if (!_childInstances.TryGetValue(parentStateMetadataId, out List<(string, string)> instances))
            {
                _childInstances[parentStateMetadataId] = instances = new List<(string, string)>();
            }

            instances.Add((instanceId, metadata.ExecutionMode == ChildStateChartExecutionMode.Remote ? metadata.RemoteUri : null));

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Inline)
            {
                var json = JObject.Parse((await childMachine.ToStringAsync()).Item2);

                Debug.Assert(json != null);

                await _orchestrationContext.CallSubOrchestratorAsync("statemachine-orchestration", instanceId, (inputs, json));
            }
            else
            {
                //TODO: remote child execution
                throw new NotImplementedException();
            }
        }

        protected override bool IsChildStateChart => _data.ContainsKey("_parentInstanceId");

        internal override Task ProcessChildStateChartDoneAsync(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            Debug.Assert(message.IsChildStateChartResponse);

            if (message.IsDone)
            {
                foreach (var pair in _childInstances.ToArray())
                {
                    pair.Value.RemoveAll(tuple => tuple.Item1 == message.CorrelationId);

                    if (pair.Value.Count == 0)
                    {
                        _childInstances.Remove(pair.Key);
                    }
                }
            }

            return Task.CompletedTask;
        }

        internal async override Task CancelInvokesAsync(string parentMetadataId)
        {
            parentMetadataId.CheckArgNull(nameof(parentMetadataId));

            var message = new ExternalMessage { Name = "cancel" };

            var childrenForParent = GetInstanceIdsForParent(parentMetadataId);

            foreach (var instanceId in childrenForParent)
            {
                await SendToChildStateChart(instanceId, message);
            }
        }

        internal override IEnumerable<string> GetInstanceIdsForParent(string parentMetadataId)
        {
            parentMetadataId.CheckArgNull(nameof(parentMetadataId));

            if (_childInstances.TryGetValue(parentMetadataId, out List<(string, string)> instances))
            {
                return instances.Select(tuple => tuple.Item1);
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        protected override Task SendMessageToParentStateChart(string _,
                                                              string messageName,
                                                              object content,
                                                              string __,
                                                              IReadOnlyDictionary<string, object> parameters,
                                                              CancellationToken ___)
        {
            messageName.CheckArgNull(nameof(messageName));

            if (!_data.TryGetValue("_parentInstanceId", out object parentInstanceId))
            {
                throw new ExecutionException("Statechart has no parent.");
            }

            Debug.Assert(parentInstanceId != null);

            var correlationId = _orchestrationContext.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(correlationId));

            var msg = new ExternalMessage
            {
                Name = messageName,
                CorrelationId = correlationId,
                Content = content,
                Parameters = parameters
            };

            if (_data.TryGetValue("_parentRemoteUri", out object parentUri))
            {
                //TODO: send message to remote parent
                throw new NotImplementedException();
            }
            else
            {
                return _orchestrationClient.RaiseEventAsync((string) parentInstanceId, "state-machine-event", msg);
            }
        }

        internal override Task SendToChildStateChart(string childInstanceId, ExternalMessage message)
        {
            childInstanceId.CheckArgNull(nameof(childInstanceId));
            message.CheckArgNull(nameof(message));

            var remoteUri = _childInstances.SelectMany(pair => pair.Value)
                                           .Where(tuple => tuple.Item1 == childInstanceId)
                                           .Select(tuple => tuple.Item2)
                                           .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(remoteUri))
            {
                return _orchestrationClient.RaiseEventAsync(childInstanceId, "state-machine-event", message);
            }
            else
            {
                //TODO: send message to remote child
                throw new NotImplementedException();
            }
        }

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

            switch (type)
            {
                case "send-parent":
                    messageName.CheckArgNull(nameof(messageName));
                    return SendMessageToParentStateChart(null, messageName, content, null, parameters, default);

                case "send-child":
                    messageName.CheckArgNull(nameof(messageName));
                    return SendMessageToChildStateChart(target, messageName, content, null, parameters, default);
            }

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
