using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Durable.Activities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class DurableExecutionContext : ExecutionContextBase
    {
        protected readonly Dictionary<string, List<(string, string)>> _childInstances;
        protected readonly OrchestrationContext _orchestrationContext;

        private readonly ExternalMessageQueue _queue;

        public DurableExecutionContext(IStateChartMetadata metadata,
                                       OrchestrationContext orchestrationContext,
                                       ExternalMessageQueue queue,
                                       IDictionary<string, object> data,
                                       CancellationToken cancelToken,
                                       ILogger logger = null)
            : base(metadata, cancelToken, logger)
        {
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            queue.CheckArgNull(nameof(queue));
            data.CheckArgNull(nameof(data));

            _orchestrationContext = orchestrationContext;
            _queue = queue;

            foreach (var pair in data)
            {
                _data[pair.Key] = pair.Value;
            }

            _childInstances = new Dictionary<string, List<(string, string)>>();
        }

        public Dictionary<string, object> ResultData => new Dictionary<string, object>(_data.Where(pair => !pair.Key.StartsWith("_")));

        protected override Task<Guid> GenerateGuid()
        {
            return _orchestrationContext.ScheduleTask<Guid>(typeof(GenerateGuidActivity), string.Empty);
        }

        internal override async Task InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentStateMetadataId)
        {
            metadata.CheckArgNull(nameof(metadata));
            parentStateMetadataId.CheckArgNull(nameof(parentStateMetadataId));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            var inputs = new Dictionary<string, object>(metadata.GetParams(this.ScriptData));

            var parentInstanceId = _orchestrationContext.OrchestrationInstance.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(parentInstanceId));

            inputs["_parentInstanceId"] = parentInstanceId;

            var metadataId = $"{GetParentStatechartMetadataId()}.{childMachine.MetadataId}";

            Debug.Assert(!string.IsNullOrWhiteSpace(metadataId));

            var instanceId = $"{metadataId}.{await GenerateGuid():N}";

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

            instances.Add((instanceId, metadata.ExecutionMode == ChildStateChartExecutionMode.Remote ? metadata.RemoteUri : null)); ;

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Inline)
            {
                await _orchestrationContext.CreateSubOrchestrationInstance<(Dictionary<string, object>, Exception)>("statechart", metadataId, instanceId, inputs);
            }
            else
            {
                await _orchestrationContext.ScheduleTask<string>("startchildorchestration", string.Empty, (metadataId, instanceId, inputs));
            }
        }

        private string GetParentStatechartMetadataId()
        {
            var parentInstanceId = _orchestrationContext.OrchestrationInstance.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(parentInstanceId));

            var idx = parentInstanceId.LastIndexOf('.');

            return parentInstanceId.Substring(0, idx);
        }

        protected override bool IsChildStateChart => _data.ContainsKey("_parentInstanceId");

        internal override async Task ProcessChildStateChartDoneAsync(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                foreach (var pair in _childInstances.ToArray())
                {
                    pair.Value.RemoveAll(tuple => tuple.Item1 == message.CorrelationId);

                    if (pair.Value.Count == 0)
                    {
                        _childInstances.Remove(pair.Key);
                    }

                    await _orchestrationContext.ScheduleTask<string>("waitforcompletion", string.Empty, message.CorrelationId);

                    return;
                }

                Debug.Fail("Expected to find child state machine instance: " + message.CorrelationId);
            }
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

            var correlationId = _orchestrationContext.OrchestrationInstance.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(correlationId));

            ExternalMessage msg = new ChildStateChartResponseMessage
            {
                Name = messageName,
                CorrelationId = correlationId,
                Content = content,
                Parameters = parameters
            };

            if (_data.TryGetValue("_parentRemoteUri", out object parentUri))
            {
                var remoteUri = (string) parentUri + (string) parentInstanceId;

                var parms = ("http-post", remoteUri, (string) null, msg, correlationId, (string) null);

                return _orchestrationContext.ScheduleTask<string>("sendmessage", string.Empty, parms);
            }
            else
            {
                return _orchestrationContext.ScheduleTask<string>("sendparentchildmessage", string.Empty, ((string) parentInstanceId, msg));
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
                return _orchestrationContext.ScheduleTask<string>("sendparentchildmessage", string.Empty, (childInstanceId, message));
            }
            else
            {
                var correlationId = _orchestrationContext.OrchestrationInstance.InstanceId;

                Debug.Assert(!string.IsNullOrWhiteSpace(correlationId));

                var queryString = new Dictionary<string, object> { { "?instanceId", childInstanceId } };

                return _orchestrationContext.ScheduleTask<string>("sendmessage",
                                                                  string.Empty,
                                                                  ("http-post", remoteUri, (string) null, message, correlationId, queryString));
            }
        }

        internal override Task DelayAsync(TimeSpan timespan)
        {
            Debug.Assert(timespan > TimeSpan.Zero);

            var expiration = _orchestrationContext.CurrentUtcDateTime.Add(timespan);

            return _orchestrationContext.CreateTimer(expiration, 0, this.CancelToken);
        }

        internal override Task<string> QueryAsync(string type, string target, IReadOnlyDictionary<string, object> parameters)
        {
            type.CheckArgNull(nameof(type));
            target.CheckArgNull(nameof(target));
            parameters.CheckArgNull(nameof(parameters));

            return _orchestrationContext.ScheduleTask<string>("query", string.Empty, (type, target, parameters));
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
                    return SendMessageToParentStateChart(null, messageName, content, null, parameters, this.CancelToken);

                case "send-child":
                    messageName.CheckArgNull(nameof(messageName));
                    return SendMessageToChildStateChart(target, messageName, content, null, parameters, this.CancelToken);
            }

            return _orchestrationContext.ScheduleTask<string>("sendmessage", string.Empty, (type, target, messageName, content, correlationId, parameters));
        }

        protected override Task<ExternalMessage> GetNextExternalMessageAsync()
        {
            return _queue.DequeueAsync(this.CancelToken);
        }

        internal override Task LogDebugAsync(string message)
        {
            Debug.Assert(_orchestrationContext != null);

            if (_logger != null)
            {
                return _orchestrationContext.ScheduleTask<bool>("logger", string.Empty, ("debug", message));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        internal override Task LogInformationAsync(string message)
        {
            Debug.Assert(_orchestrationContext != null);

            if (_logger != null)
            {
                return _orchestrationContext.ScheduleTask<bool>("logger", string.Empty, ("information", message));
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
