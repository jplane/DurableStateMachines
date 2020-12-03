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
        private readonly List<string> _childInstances;
        private readonly OrchestrationContext _orchestrationContext;

        private TaskCompletionSource<ExternalMessage> _externalMessageSource;

        public DurableExecutionContext(IRootStateMetadata metadata,
                                       OrchestrationContext orchestrationContext,
                                       IDictionary<string, object> data,
                                       ILogger logger = null)
            : base(metadata, logger)
        {
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            data.CheckArgNull(nameof(data));

            _orchestrationContext = orchestrationContext;
            
            foreach (var pair in data)
            {
                _data[pair.Key] = pair.Value;
            }

            _childInstances = new List<string>();
            _externalMessageSource = new TaskCompletionSource<ExternalMessage>();
        }

        public Dictionary<string, object> ResultData => new Dictionary<string, object>(_data.Where(pair => !pair.Key.StartsWith("_")));

        protected override Task<Guid> GenerateGuid()
        {
            return _orchestrationContext.ScheduleTask<Guid>(typeof(GenerateGuidActivity), string.Empty);
        }

        protected override async Task<ExternalMessage> GetNextExternalMessageAsync()
        {
            var message = await _externalMessageSource.Task;

            _externalMessageSource = new TaskCompletionSource<ExternalMessage>();

            return message;
        }

        internal override Task InvokeChildStateChart(IInvokeStateChartMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            var invokeId = metadata.UniqueId;

            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));

            var parentInvokeId = _orchestrationContext.OrchestrationInstance.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(parentInvokeId));

            var inputs = new Dictionary<string, object>(metadata.GetParams(this.ScriptData));

            inputs["_parentInvokeId"] = parentInvokeId;

            inputs["_invokeId"] = invokeId;

            _childInstances.Add(invokeId);

            return _orchestrationContext.ScheduleTask<string>("startchildorchestration", invokeId, (invokeId, inputs));
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

            return _childInstances.Where(invokeId => invokeId.StartsWith($"{parentUniqueId}.")).ToArray();
        }

        internal override async Task ProcessChildStateChartDoneAsync(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                Debug.Assert(_childInstances.Contains(message.CorrelationId), "Expected to find child state machine instance: " + message.CorrelationId);

                await _orchestrationContext.ScheduleTask<string>("waitforcompletion", string.Empty, message.CorrelationId);
            }
        }

        public override Task SendAsync(ExternalMessage message)
        {
            throw new NotImplementedException();    // should never be called
        }

        protected override async Task SendMessageToParentStateChart(string _,
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

            var msg = new ChildStateChartResponseMessage(messageName)
            {
                CorrelationId = correlationId,
                Content = content,
                Parameters = parameters
            };

            await _orchestrationContext.ScheduleTask<string>("sendparentchildmessage", string.Empty, ((string) parentInvokeId, (ExternalMessage) msg));
        }

        internal override void InternalCancel()
        {
            Debug.Assert(_externalMessageSource != null);

            if (!_data.ContainsKey("_parentInvokeId"))
            {
                _externalMessageSource.SetResult(new ExternalMessage("cancel"));
            }
        }

        internal override Task SendToChildStateChart(string invokeId, ExternalMessage message)
        {
            invokeId.CheckArgNull(nameof(invokeId));
            message.CheckArgNull(nameof(message));

            return _orchestrationContext.ScheduleTask<string>("sendparentchildmessage", string.Empty, (invokeId, message));
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

            return _orchestrationContext.ScheduleTask<object>("script", metadata.UniqueId, _data);
        }

        internal void EnqueueExternalMessage(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            Debug.Assert(_externalMessageSource != null);

            _externalMessageSource.SetResult(message);
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
