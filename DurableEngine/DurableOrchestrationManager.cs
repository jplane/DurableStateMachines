using DurableTask.Core;
using DurableTask.Core.Serializing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    internal interface IStateChartOrchestrationManager
    {
        Task StartOrchestrationAsync(string instanceId, IRootStateMetadata metadata, Dictionary<string, object> data);
        Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId, TimeSpan timeout);
        Task SendMessageAsync(string instanceId, ExternalMessage message);
    }

    internal class DurableOrchestrationManager : IStateChartOrchestrationManager
    {
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly NameVersionObjectManager<TaskOrchestration> _orchestrationResolver;
        private readonly NameVersionObjectManager<TaskActivity> _activityResolver;
        private readonly IOrchestrationService _orchestrationService;
        private readonly TimeSpan _timeout;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancelToken;

        private TaskHubWorker _worker;

        public DurableOrchestrationManager(IOrchestrationService orchestrationService,
                                           TimeSpan timeout,
                                           CancellationToken cancelToken,
                                           ILogger logger = null)
        {
            orchestrationService.CheckArgNull(nameof(orchestrationService));

            _orchestrationService = orchestrationService;
            _logger = logger;
            _cancelToken = cancelToken;
            _timeout = timeout;

            _orchestrationResolver = new NameVersionObjectManager<TaskOrchestration>();
            _activityResolver = new NameVersionObjectManager<TaskActivity>();

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", HttpService.PostAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", HttpService.GetAsync);
        }

        public Task StartAsync()
        {
            if (_worker != null)
            {
                throw new InvalidOperationException("Service already started.");
            }

            _worker = new TaskHubWorker(_orchestrationService, _orchestrationResolver, _activityResolver);

            return _worker.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_worker == null)
            {
                throw new InvalidOperationException("Service not started.");
            }

            _orchestrationResolver.Clear();
            _activityResolver.Clear();

            await _worker.StopAsync();

            _worker = null;
        }

        public Task StartOrchestrationAsync(string instanceId, ExecutionContext context)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            context.CheckArgNull(nameof(context));

            if (_worker == null)
            {
                throw new InvalidOperationException("Service not started.");
            }

            RegisterMetadata(context.Metadata);

            AddTaskActivities();

            return StartOrchestrationAsync(instanceId, context.Metadata, context.Data);
        }

        public Task StartOrchestrationAsync(string instanceId, IRootStateMetadata metadata, Dictionary<string, object> data)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            metadata.CheckArgNull(nameof(metadata));
            data.CheckArgNull(nameof(data));

            if (_worker == null)
            {
                throw new InvalidOperationException("Service already started.");
            }

            var client = new TaskHubClient((IOrchestrationServiceClient)_orchestrationService);

            return client.CreateOrchestrationInstanceAsync("statechart", instanceId, instanceId, data);
        }

        public Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId)
        {
            return WaitForCompletionAsync(instanceId, _timeout);
        }

        public async Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId, TimeSpan timeout)
        {
            if (_worker == null)
            {
                throw new InvalidOperationException("Service already started.");
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
            message.CheckArgNull(nameof(message));

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            return client.RaiseEventAsync(instance, message.Name, message);
        }

        private void RegisterStateChart(string instanceId, IRootStateMetadata metadata)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            metadata.CheckArgNull(nameof(metadata));

            // this is the durable activity for starting a child statechart from within its parent

            var createActivity = new CreateChildOrchestrationActivity(metadata, this);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("startchildorchestration", instanceId, createActivity);

            _activityResolver.Add(activityCreator);

            // this is the orchestration that runs a statechart instance (parent or child)

            var orchestrator = new InterpreterOrchestration(metadata, _cancelToken, _logger);

            var orchestrationCreator = new NameValueObjectCreator<TaskOrchestration>("statechart", instanceId, orchestrator);

            _orchestrationResolver.Add(orchestrationCreator);
        }

        private void RegisterScript(IScriptMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var scriptActivity = new ExecuteScriptActivity(metadata);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("script", metadata.UniqueId, scriptActivity);

            _activityResolver.Add(activityCreator);
        }

        private void RegisterMetadata(IRootStateMetadata metadata)
        {
            Debug.Assert(metadata != null);

            RegisterStateChart(metadata.UniqueId, metadata);

            metadata.RegisterStateChartInvokes((id, root) => RegisterStateChart(id, root));

            metadata.RegisterScripts(RegisterScript);
        }

        private void AddTaskActivities()
        {
            _worker.AddTaskActivities(typeof(GenerateGuidActivity));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendparentchildmessage",
                                                                               string.Empty,
                                                                               new SendParentChildMessageActivity(this)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("waitforcompletion",
                                                                               string.Empty,
                                                                               new WaitForCompletionActivity(this, _timeout / 2)));

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
