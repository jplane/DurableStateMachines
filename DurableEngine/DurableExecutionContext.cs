using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class DurableExecutionContext : StateChartsDotNet.ExecutionContext
    {
        private readonly Action<string, Func<TaskActivity>> _ensureActivityRegistration;
        private readonly Action<string, Func<InterpreterOrchestration>> _ensureOrchestrationRegistration;
        private readonly OrchestrationContext _orchestrationContext;
        private readonly List<string> _childInstances;

        public DurableExecutionContext(IRootStateMetadata metadata,
                                       OrchestrationContext orchestrationContext,
                                       Action<string, Func<TaskActivity>> ensureActivityRegistration,
                                       Action<string, Func<InterpreterOrchestration>> ensureOrchestrationRegistration,
                                       IReadOnlyDictionary<string, IRootStateMetadata> childMetadata,
                                       IReadOnlyDictionary<string, ExternalServiceDelegate> externalServices,
                                       IReadOnlyDictionary<string, ExternalQueryDelegate> externalQueries,
                                       ILogger logger = null)
            : base(metadata, logger)
        {
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            ensureActivityRegistration.CheckArgNull(nameof(ensureActivityRegistration));
            ensureOrchestrationRegistration.CheckArgNull(nameof(ensureOrchestrationRegistration));
            childMetadata.CheckArgNull(nameof(childMetadata));
            externalServices.CheckArgNull(nameof(externalServices));
            externalQueries.CheckArgNull(nameof(externalQueries));

            _orchestrationContext = orchestrationContext;
            _ensureActivityRegistration = ensureActivityRegistration;
            _ensureOrchestrationRegistration = ensureOrchestrationRegistration;

            _childInstances = new List<string>();

            _childMetadata.AddRange(childMetadata);
            _externalServices.AddRange(externalServices);
            _externalQueries.AddRange(externalQueries);
        }

        internal IDictionary<string, object> GetData() => _data;

        internal override async Task InitAsync()
        {
            Debug.Assert(_orchestrationContext != null);

            this["_sessionid"] = (await _orchestrationContext.ScheduleTask<Guid>(typeof(GenerateGuidActivity), string.Empty)).ToString("D");

            this["_name"] = this.Root.Name;
        }

        internal async override Task InvokeChildStateChart(IInvokeStateChartMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var invokeId = await ResolveInvokeId(metadata);

            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            _ensureOrchestrationRegistration(childMachine.Id, () => CreateOrchestration(childMachine));

            var parameters = new Dictionary<string, object>
            {
                { "_parentInstance", _orchestrationContext.OrchestrationInstance }
            };

            parameters.AddRange(metadata.GetParams(this.ScriptData));

            await _orchestrationContext.CreateSubOrchestrationInstance<InterpreterOrchestration>("statechart",
                                                                                                 childMachine.Id,
                                                                                                 invokeId,
                                                                                                 parameters);

            _childInstances.Add(invokeId);
        }

        private InterpreterOrchestration CreateOrchestration(IRootStateMetadata metadata)
        {
            Debug.Assert(metadata != null);

            return new InterpreterOrchestration(metadata,
                                                _ensureActivityRegistration,
                                                _ensureOrchestrationRegistration,
                                                _childMetadata,
                                                _externalServices,
                                                _externalQueries,
                                                _logger);
        }

        internal override Task SendDoneMessageToParent(object content,
                                                       IReadOnlyDictionary<string, object> parameters)
        {
            if (_data.TryGetValue("_parentInstance", out object parentInstance))
            {
                var instance = (OrchestrationInstance) parentInstance;

                return SendMessageToParentStateChart(null, $"done.invoke.{instance.InstanceId}", content, null, parameters);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        protected override Task SendMessageToParentStateChart(string _,
                                                              string messageName,
                                                              object content,
                                                              string __,
                                                              IReadOnlyDictionary<string, object> parameters)
        {
            messageName.CheckArgNull(nameof(messageName));

            if (_data.TryGetValue("_parentInstance", out object parentInstance))
            {
                var msg = new ChildStateChartResponseMessage(messageName)
                {
                    CorrelationId = _orchestrationContext.OrchestrationInstance.InstanceId,
                    Content = content,
                    Parameters = parameters
                };

                _orchestrationContext.SendEvent((OrchestrationInstance) parentInstance, messageName, msg);

                return Task.CompletedTask;
            }
            else
            {
                throw new InvalidOperationException("Current statechart has no parent.");
            }
        }

        protected async override Task<string> ResolveInvokeId(IInvokeStateChartMetadata metadata)
        {
            Debug.Assert(metadata != null);

            var invokeId = metadata.Id;

            if (string.IsNullOrWhiteSpace(invokeId))
            {
                var guid = await _orchestrationContext.ScheduleTask<Guid>(typeof(GenerateGuidActivity), string.Empty);

                invokeId = $"{metadata.UniqueId}.{guid.ToString("N")}";

                await this.LogDebugAsync($"Synthentic Id = {invokeId}");

                if (!string.IsNullOrWhiteSpace(metadata.IdLocation))
                {
                    _data[metadata.IdLocation] = invokeId;
                }
            }

            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));

            Debug.Assert(!_childInstances.Contains(invokeId));

            return invokeId;
        }

        internal override Task CancelInvokesAsync(string parentUniqueId)
        {
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));

            foreach (var invokeId in _childInstances.Where(id => id.StartsWith($"{parentUniqueId}.")).ToArray())
            {
                var instance = new OrchestrationInstance
                {
                    InstanceId = invokeId,
                    ExecutionId = null
                };

                _orchestrationContext.SendEvent(instance, "cancel", null);

                _childInstances.Remove(invokeId);
            }

            return Task.CompletedTask;
        }

        internal async override Task ProcessExternalMessageAsync(string parentUniqueId, InvokeStateChart invoke, ExternalMessage message)
        {
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));
            invoke.CheckArgNull(nameof(invoke));

            foreach (var invokeId in _childInstances.Where(id => id.StartsWith($"{parentUniqueId}.")).ToArray())
            {
                await invoke.ProcessExternalMessageAsync(invokeId, this, message);
            }
        }

        internal override void ProcessChildStateChartDone(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                Debug.Assert(_childInstances.Contains(message.CorrelationId));

                _childInstances.Remove(message.CorrelationId);
            }
        }

        internal override Task SendToChildStateChart(string invokeId, ExternalMessage message)
        {
            invokeId.CheckArgNull(nameof(invokeId));
            message.CheckArgNull(nameof(message));

            Debug.Assert(_childInstances.Contains(invokeId));

            var instance = new OrchestrationInstance
            {
                InstanceId = invokeId,
                ExecutionId = null
            };

            _orchestrationContext.SendEvent(instance, message.Name, message);

            return Task.CompletedTask;
        }

        internal override Task DelayAsync(TimeSpan timespan)
        {
            Debug.Assert(timespan > TimeSpan.Zero);

            var expiration = _orchestrationContext.CurrentUtcDateTime.Add(timespan);

            return _orchestrationContext.CreateTimer(expiration, 0);
        }

        internal override Task ExecuteContentAsync(string uniqueId, Func<StateChartsDotNet.ExecutionContext, Task> func)
        {
            uniqueId.CheckArgNull(nameof(func));
            func.CheckArgNull(nameof(func));

            Debug.Assert(_orchestrationContext != null);

            _ensureActivityRegistration(uniqueId, () => new ExecutableContentActivity(func, this));

            return _orchestrationContext.ScheduleTask<bool>(uniqueId, string.Empty);
        }

        internal Task EnqueueExternalMessage(ExternalMessage message)
        {
            base.Send(message);

            return Task.CompletedTask;
        }

        internal override void Send(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            Debug.Assert(_orchestrationContext != null);

            _orchestrationContext.SendEvent(_orchestrationContext.OrchestrationInstance, message.Name, message);
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
