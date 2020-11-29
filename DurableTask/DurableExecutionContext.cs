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

namespace StateChartsDotNet.DurableTask
{
    public class DurableExecutionContext : ExecutionContext
    {
        private readonly Action<string, Func<TaskActivity>> _ensureActivityRegistration;
        private readonly OrchestrationContext _orchestrationContext;
        private readonly IOrchestrationServiceClient _orchestrationClient;
        private readonly Dictionary<string, DurableStateChartClient> _childInstances;
        private readonly Action<string, Func<InterpreterOrchestration>> _ensureOrchestrationRegistration;

        public DurableExecutionContext(IRootStateMetadata metadata,
                                       OrchestrationContext orchestrationContext,
                                       Action<string, Func<TaskActivity>> ensureActivityRegistration,
                                       Action<string, Func<InterpreterOrchestration>> ensureOrchestrationRegistration,
                                       Dictionary<string, IRootStateMetadata> childMetadata,
                                       Dictionary<string, ExternalServiceDelegate> externalServices,
                                       Dictionary<string, ExternalQueryDelegate> externalQueries,
                                       IOrchestrationServiceClient orchestrationClient,
                                       ILogger logger = null)
            : base(metadata, logger)
        {
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            ensureActivityRegistration.CheckArgNull(nameof(ensureActivityRegistration));
            ensureOrchestrationRegistration.CheckArgNull(nameof(ensureOrchestrationRegistration));
            childMetadata.CheckArgNull(nameof(childMetadata));
            externalServices.CheckArgNull(nameof(externalServices));
            externalQueries.CheckArgNull(nameof(externalQueries));
            orchestrationClient.CheckArgNull(nameof(orchestrationClient));

            _orchestrationContext = orchestrationContext;
            _ensureActivityRegistration = ensureActivityRegistration;
            _ensureOrchestrationRegistration = ensureOrchestrationRegistration;
            _orchestrationClient = orchestrationClient;

            _childInstances = new Dictionary<string, DurableStateChartClient>();

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

            var client = new DurableStateChartClient(_orchestrationClient, childMachine.Id, invokeId);

            client["_parentDurableInstanceId"] = _orchestrationContext.OrchestrationInstance.InstanceId;

            client["_invokeId"] = invokeId;

            foreach (var pair in metadata.GetParams(this.ScriptData))
            {
                client[pair.Key] = pair.Value;
            }

            await client.InitAsync();

            _childInstances.Add(invokeId, client);
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
                                                _orchestrationClient,
                                                _logger);
        }

        protected override async Task SendMessageToParentStateChart(string _,
                                                                    string messageName,
                                                                    object content,
                                                                    string __,
                                                                    IReadOnlyDictionary<string, object> parameters)
        {
            messageName.CheckArgNull(nameof(messageName));

            if (_data.TryGetValue("_parentDurableInstanceId", out object parentInstanceId))
            {
                await DurableStateChartClient.SendMessageToParent(_orchestrationClient,
                                                                  (string) parentInstanceId,
                                                                  messageName,
                                                                  (string) _data["_invokeId"],
                                                                  content,
                                                                  parameters);
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

            Debug.Assert(!_childInstances.ContainsKey(invokeId));

            return invokeId;
        }

        internal async override Task CancelInvokesAsync(string parentUniqueId)
        {
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));

            foreach (var pair in _childInstances.Where(p => p.Key.StartsWith($"{parentUniqueId}.")).ToArray())
            {
                var invokeId = pair.Key;
                var client = pair.Value;

                Debug.Assert(client != null);

                await client.StopAsync();

                _childInstances.Remove(invokeId);
            }
        }

        internal async override Task ProcessExternalMessageAsync(string parentUniqueId, InvokeStateChart invoke, ExternalMessage message)
        {
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));
            invoke.CheckArgNull(nameof(invoke));

            foreach (var pair in _childInstances.Where(p => p.Key.StartsWith($"{parentUniqueId}.")).ToArray())
            {
                var invokeId = pair.Key;

                await invoke.ProcessExternalMessageAsync(invokeId, this, message);
            }
        }

        internal override void ProcessChildStateChartDone(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                Debug.Assert(_childInstances.ContainsKey(message.CorrelationId));

                _childInstances.Remove(message.CorrelationId);
            }
        }

        internal async override Task SendToChildStateChart(string id, ExternalMessage message)
        {
            id.CheckArgNull(nameof(id));
            message.CheckArgNull(nameof(message));

            if (_childInstances.TryGetValue(id, out DurableStateChartClient client))
            {
                await client.SendMessageAsync(message);
            }
            else
            {
                Debug.Fail($"Unable to find child statechart {id}.");
            }
        }

        internal override Task ExecuteContentAsync(string uniqueId, Func<ExecutionContext, Task> func)
        {
            uniqueId.CheckArgNull(nameof(func));
            func.CheckArgNull(nameof(func));

            Debug.Assert(_orchestrationContext != null);

            _ensureActivityRegistration(uniqueId, () => new ExecutableContentActivity(func, this));

            return _orchestrationContext.ScheduleTask<bool>(uniqueId, string.Empty);
        }

        internal void EnqueueExternalMessage(ExternalMessage message)
        {
            base.Send(message);
        }

        public override void Send(ExternalMessage message)
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
