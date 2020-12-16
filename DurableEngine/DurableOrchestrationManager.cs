using DurableTask.Core;
using DurableTask.Core.Serializing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nito.AsyncEx;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Durable.Activities;
using StateChartsDotNet.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal interface IOrchestrationManager
    {
        Task StartAsync();

        Task StopAsync();

        Task StartOrchestrationAsync(IRootStateMetadata metadata,
                                     string metadataId,
                                     string instanceId,
                                     IDictionary<string, object> data,
                                     CancellationToken cancelToken,
                                     bool isRoot = false);

        Task StartOrchestrationAsync(IRootStateMetadata metadata,
                                     string metadataId,
                                     string instanceId,
                                     IDictionary<string, object> data,
                                     bool isRoot = false);

        Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId);

        Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId, CancellationToken cancelToken);

        Task SendMessageAsync(string instanceId, ExternalMessage message);
    }

    internal class DurableOrchestrationManager : IOrchestrationManager
    {
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly NameVersionObjectManager<TaskOrchestration> _orchestrationResolver;
        private readonly NameVersionObjectManager<TaskActivity> _activityResolver;
        private readonly IOrchestrationService _orchestrationService;
        private readonly ILogger _logger;
        private readonly TimeSpan _timeout;
        private readonly AsyncLock _lock;

        private TaskHubWorker _worker;

        public DurableOrchestrationManager(IOrchestrationService orchestrationService,
                                           TimeSpan timeout,
                                           ILogger logger = null)
        {
            orchestrationService.CheckArgNull(nameof(orchestrationService));

            if (!(orchestrationService is IOrchestrationServiceClient))
            {
                throw new ArgumentException("Expecting orchestration service to implement both client and service interfaces.");
            }

            _orchestrationService = orchestrationService;
            _timeout = timeout;
            _logger = logger;

            _lock = new AsyncLock();

            _orchestrationResolver = new NameVersionObjectManager<TaskOrchestration>();
            _activityResolver = new NameVersionObjectManager<TaskActivity>();

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", HttpService.PostAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", HttpService.GetAsync);
        }

        public async Task StartAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_worker == null)
                {
                    _worker = new TaskHubWorker(_orchestrationService, _orchestrationResolver, _activityResolver);

                    await _worker.StartAsync();
                }
            }
        }

        public async Task StopAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_worker != null)
                {
                    _orchestrationResolver.Clear();
                    _activityResolver.Clear();

                    await _worker.StopAsync();

                    _worker = null;
                }
            }
        }

        public Task StartOrchestrationAsync(IRootStateMetadata metadata,
                                            string metadataId,
                                            string instanceId,
                                            IDictionary<string, object> data,
                                            bool isRoot = false)
        {
            return StartOrchestrationAsync(metadata, metadataId, instanceId, data, CancellationToken.None, isRoot);
        }

        public async Task StartOrchestrationAsync(IRootStateMetadata metadata,
                                                  string metadataId,
                                                  string instanceId,
                                                  IDictionary<string, object> data,
                                                  CancellationToken cancelToken,
                                                  bool isRoot = false)
        {
            metadata.CheckArgNull(nameof(metadata));
            metadataId.CheckArgNull(nameof(metadataId));
            instanceId.CheckArgNull(nameof(instanceId));
            data.CheckArgNull(nameof(data));

            using (await _lock.LockAsync())
            {
                if (_worker == null)
                {
                    throw new InvalidOperationException("Service already started.");
                }

                if (isRoot)
                {
                    RegisterMetadata(metadata, cancelToken);

                    AddTaskActivities(cancelToken);
                }

                var client = new TaskHubClient((IOrchestrationServiceClient)_orchestrationService);

                await client.CreateOrchestrationInstanceAsync("statechart", metadataId, instanceId, data);
            }
        }

        public Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId)
        {
            return WaitForCompletionAsync(instanceId, CancellationToken.None);
        }

        public async Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId, CancellationToken cancelToken)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            var result = await client.WaitForOrchestrationAsync(instance, _timeout, cancelToken);

            var dataconverter = new JsonDataConverter(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var tuple = dataconverter.Deserialize<(Dictionary<string, object>, Exception)>(result.Output);

            if (tuple.Item2 != null)
            {
                ExceptionDispatchInfo.Capture(tuple.Item2).Throw();
                return null;
            }
            else
            {
                return tuple.Item1;
            }
        }

        public Task SendMessageAsync(string instanceId, ExternalMessage message)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            message.CheckArgNull(nameof(message));

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            return client.RaiseEventAsync(instance, message.Name, message);
        }

        private void RegisterStateChart(string uniqueId, IRootStateMetadata metadata, CancellationToken cancelToken, bool executeInline = false)
        {
            uniqueId.CheckArgNull(nameof(uniqueId));
            metadata.CheckArgNull(nameof(metadata));

            // this is the durable activity for starting a child statechart from within its parent

            var createActivity = new CreateChildOrchestrationActivity(metadata, uniqueId, this, cancelToken);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("startchildorchestration", uniqueId, createActivity);

            _activityResolver.Add(activityCreator);

            // this is the orchestration that runs a statechart instance (parent or child)

            var orchestrator = new InterpreterOrchestration(metadata, cancelToken, executeInline, _logger);

            var orchestrationCreator = new NameValueObjectCreator<TaskOrchestration>("statechart", uniqueId, orchestrator);

            _orchestrationResolver.Add(orchestrationCreator);
        }

        private void RegisterScript(IScriptMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var scriptActivity = new ExecuteScriptActivity(metadata);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("script", metadata.UniqueId, scriptActivity);

            _activityResolver.Add(activityCreator);
        }

        private void RegisterMetadata(IRootStateMetadata metadata, CancellationToken cancelToken)
        {
            Debug.Assert(metadata != null);

            RegisterStateChart(metadata.UniqueId, metadata, cancelToken);

            metadata.RegisterStateChartInvokes((id, root, inline) => RegisterStateChart(id, root, cancelToken, inline));

            metadata.RegisterScripts(RegisterScript);
        }

        private void AddTaskActivities(CancellationToken cancelToken)
        {
            _worker.AddTaskActivities(typeof(GenerateGuidActivity));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendparentchildmessage",
                                                                               string.Empty,
                                                                               new SendParentChildMessageActivity(this)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("waitforcompletion",
                                                                               string.Empty,
                                                                               new WaitForCompletionActivity(this, cancelToken)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("logger", string.Empty, new LoggerActivity(_logger)));

            Func<string, ExternalServiceDelegate> getService = name =>
            {
                _externalServices.TryGetValue(name, out ExternalServiceDelegate func);
                return func;
            };

            Func<string, ExternalQueryDelegate> getQuery = name =>
            {
                _externalQueries.TryGetValue(name, out ExternalQueryDelegate func);
                return func;
            };

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("query", string.Empty, new QueryActivity(getQuery, cancelToken)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendmessage", string.Empty, new SendMessageActivity(getService, cancelToken)));
        }
    }
}
