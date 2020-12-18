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

        Task StartOrchestrationAsync(string metadataId, string instanceId, IDictionary<string, object> data);

        Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId);

        Task SendMessageAsync(string instanceId, ExternalMessage message);
    }

    internal class DurableOrchestrationManager : IOrchestrationManager
    {
        private readonly IStateChartMetadata _metadata;
        private readonly Dictionary<string, IStateChartMetadata> _metadataMap;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly NameVersionObjectManager<TaskOrchestration> _orchestrationResolver;
        private readonly NameVersionObjectManager<TaskActivity> _activityResolver;
        private readonly IOrchestrationService _orchestrationService;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancelToken;
        private readonly TimeSpan _timeout;
        private readonly AsyncLock _lock;

        private TaskHubWorker _worker;

        public DurableOrchestrationManager(IStateChartMetadata metadata,
                                           IOrchestrationService orchestrationService,
                                           TimeSpan timeout,
                                           CancellationToken cancelToken,
                                           ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationService.CheckArgNull(nameof(orchestrationService));

            if (!(orchestrationService is IOrchestrationServiceClient))
            {
                throw new ArgumentException("Expecting orchestration service to implement both client and service interfaces.");
            }

            _metadata = metadata;
            _orchestrationService = orchestrationService;
            _timeout = timeout;
            _cancelToken = cancelToken;
            _logger = logger;

            _lock = new AsyncLock();
            _metadataMap = new Dictionary<string, IStateChartMetadata>();

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

                    RegisterMetadata();

                    AddTaskActivities();
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
                    _metadataMap.Clear();

                    await _worker.StopAsync();

                    _worker = null;
                }
            }
        }

        public async Task StartOrchestrationAsync(string metadataId,
                                                  string instanceId,
                                                  IDictionary<string, object> data)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            instanceId.CheckArgNull(nameof(instanceId));
            data.CheckArgNull(nameof(data));

            if (_worker == null)
            {
                throw new InvalidOperationException("Service not started.");
            }

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            await client.CreateOrchestrationInstanceAsync("statechart", metadataId, instanceId, data);
        }

        public async Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            if (_worker == null)
            {
                throw new InvalidOperationException("Service not started.");
            }

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            var result = await client.WaitForOrchestrationAsync(instance, _timeout, _cancelToken);

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

            if (_worker == null)
            {
                throw new InvalidOperationException("Service not started.");
            }

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            return client.RaiseEventAsync(instance, message.Name, message);
        }

        private void RegisterStateChart(string uniqueId, IStateChartMetadata metadata, bool executeInline = false)
        {
            uniqueId.CheckArgNull(nameof(uniqueId));
            metadata.CheckArgNull(nameof(metadata));

            // this is the durable activity for starting a child statechart from within its parent

            var createActivity = new CreateChildOrchestrationActivity(uniqueId, this);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("startchildorchestration", uniqueId, createActivity);

            _activityResolver.Add(activityCreator);

            // this is the orchestration that runs a statechart instance (parent or child)

            var orchestrator = new InterpreterOrchestration(metadata, _cancelToken, executeInline, _logger);

            var orchestrationCreator = new NameValueObjectCreator<TaskOrchestration>("statechart", uniqueId, orchestrator);

            _orchestrationResolver.Add(orchestrationCreator);

            _metadataMap.Add(uniqueId, metadata);
        }

        private void RegisterScript(IScriptMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var scriptActivity = new ExecuteScriptActivity(metadata);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("script", metadata.UniqueId, scriptActivity);

            _activityResolver.Add(activityCreator);
        }

        private void RegisterMetadata()
        {
            Debug.Assert(_metadata != null);

            RegisterStateChart(_metadata.UniqueId, _metadata);

            _metadata.RegisterStateChartInvokes((id, root, inline) => RegisterStateChart(id, root, inline));

            _metadata.RegisterScripts(RegisterScript);
        }

        private void AddTaskActivities()
        {
            _worker.AddTaskActivities(typeof(GenerateGuidActivity));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendparentchildmessage",
                                                                               string.Empty,
                                                                               new SendParentChildMessageActivity(this)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("waitforcompletion",
                                                                               string.Empty,
                                                                               new WaitForCompletionActivity(this)));

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

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("query", string.Empty, new QueryActivity(getQuery, _cancelToken)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendmessage", string.Empty, new SendMessageActivity(getService, _cancelToken)));
        }
    }
}
