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
using System.Xml.Xsl;

namespace StateChartsDotNet.Durable
{
    internal class DurableExecutionContext : ExecutionContextBase
    {
        protected readonly Dictionary<string, List<string>> _childInstances;
        protected readonly OrchestrationContext _orchestrationContext;

        private readonly Queue<ExternalMessage> _externalMessages;

        private TaskCompletionSource<bool> _externalMessageAvailable;

        public DurableExecutionContext(IStateChartMetadata metadata,
                                       OrchestrationContext orchestrationContext,
                                       IDictionary<string, object> data,
                                       CancellationToken cancelToken,
                                       ILogger logger = null)
            : base(metadata, cancelToken, logger)
        {
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));

            _orchestrationContext = orchestrationContext;
            
            foreach (var pair in data)
            {
                _data[pair.Key] = pair.Value;
            }

            _childInstances = new Dictionary<string, List<string>>();
            _externalMessageAvailable = new TaskCompletionSource<bool>();
            _externalMessages = new Queue<ExternalMessage>();
        }

        public Dictionary<string, object> ResultData => new Dictionary<string, object>(_data.Where(pair => !pair.Key.StartsWith("_")));

        protected override Task<Guid> GenerateGuid()
        {
            return _orchestrationContext.ScheduleTask<Guid>(typeof(GenerateGuidActivity), string.Empty);
        }

        internal override async Task InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentUniqueId)
        {
            metadata.CheckArgNull(nameof(metadata));
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            var inputs = new Dictionary<string, object>(metadata.GetParams(this.ScriptData));

            var parentInvokeId = _orchestrationContext.OrchestrationInstance.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(parentInvokeId));

            inputs["_parentInvokeId"] = parentInvokeId;

            var invokeId = $"{GetAncestorUniqueId()}.{childMachine.UniqueId}.{await GenerateGuid():N}";

            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));

            inputs["_invokeId"] = invokeId;

            if (! _childInstances.TryGetValue(parentUniqueId, out List<string> instances))
            {
                _childInstances[parentUniqueId] = instances = new List<string>();
            }

            instances.Add(invokeId);

            await StartChildOrchestrationAsync($"{GetAncestorUniqueId()}.{childMachine.UniqueId}", invokeId, inputs);
        }

        private string GetAncestorUniqueId()
        {
            var parentInvokeId = _orchestrationContext.OrchestrationInstance.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(parentInvokeId));

            var idx = parentInvokeId.LastIndexOf('.');

            return parentInvokeId.Substring(0, idx);
        }

        protected override bool IsChildStateChart => _data.ContainsKey("_parentInvokeId");

        protected virtual Task StartChildOrchestrationAsync(string uniqueId, string invokeId, Dictionary<string, object> data)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(uniqueId));
            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));
            Debug.Assert(data != null);

            Debug.Assert(_orchestrationContext != null);

            return _orchestrationContext.ScheduleTask<string>("startchildorchestration", uniqueId, (invokeId, data));
        }

        internal override async Task ProcessChildStateChartDoneAsync(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                foreach (var pair in _childInstances.ToArray())
                {
                    if (pair.Value.Remove(message.CorrelationId))
                    {
                        if (pair.Value.Count == 0)
                        {
                            _childInstances.Remove(pair.Key);
                        }

                        await _orchestrationContext.ScheduleTask<string>("waitforcompletion", string.Empty, message.CorrelationId);
                        
                        return;
                    }
                }

                Debug.Fail("Expected to find child state machine instance: " + message.CorrelationId);
            }
        }

        internal async override Task CancelInvokesAsync(string parentUniqueId)
        {
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));

            var message = new ExternalMessage("cancel");

            var childrenForParent = GetInvokeIdsForParent(parentUniqueId);

            foreach (var invokeId in childrenForParent)
            {
                await SendToChildStateChart(invokeId, message);
            }
        }

        internal override IEnumerable<string> GetInvokeIdsForParent(string parentUniqueId)
        {
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));

            if (_childInstances.TryGetValue(parentUniqueId, out List<string> instances))
            {
                return instances;
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

            if (!_data.TryGetValue("_parentInvokeId", out object parentInvokeId))
            {
                throw new ExecutionException("Statechart has no parent.");
            }

            Debug.Assert(parentInvokeId != null);

            var correlationId = _orchestrationContext.OrchestrationInstance.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(correlationId));

            ExternalMessage msg = new ChildStateChartResponseMessage(messageName)
            {
                CorrelationId = correlationId,
                Content = content,
                Parameters = parameters
            };

            return _orchestrationContext.ScheduleTask<string>("sendparentchildmessage", string.Empty, ((string) parentInvokeId, msg));
        }

        internal override Task SendToChildStateChart(string childInvokeId, ExternalMessage message)
        {
            childInvokeId.CheckArgNull(nameof(childInvokeId));
            message.CheckArgNull(nameof(message));

            return _orchestrationContext.ScheduleTask<string>("sendparentchildmessage", string.Empty, (childInvokeId, message));
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

        internal override Task ExecuteScriptAsync(IScriptMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var uniqueId = $"{GetAncestorUniqueId()}.{metadata.UniqueId}";

            return _orchestrationContext.ScheduleTask<object>("script", uniqueId, _data);
        }

        protected override async Task<ExternalMessage> GetNextExternalMessageAsync()
        {
            using (this.CancelToken.Register(() => _externalMessageAvailable.SetCanceled()))
            {
                await _externalMessageAvailable.Task;
            }

            var msg = _externalMessages.Dequeue();

            if (_externalMessages.Count == 0)
            {
                _externalMessageAvailable = new TaskCompletionSource<bool>();
            }

            return msg;
        }

        internal void EnqueueExternalMessage(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            _externalMessages.Enqueue(message);

            _externalMessageAvailable.TrySetResult(true);
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
