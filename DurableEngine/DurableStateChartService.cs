using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Durable.Activities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class DurableStateChartService
    {
        private readonly IOrchestrationService _service;
        private readonly IStateChartOrchestrationManager _orchestrationManager;
        private readonly ILogger _logger;
        private readonly NameVersionObjectManager<TaskOrchestration> _orchestrationResolver;
        private readonly NameVersionObjectManager<TaskActivity> _activityResolver;
        private readonly Func<string, ExternalServiceDelegate> _getServices;
        private readonly Func<string, ExternalQueryDelegate> _getQueries;
        private readonly TimeSpan _timeout;

        private TaskHubWorker _worker;

        public DurableStateChartService(IOrchestrationService service,
                                        IStateChartOrchestrationManager orchestrationManager,
                                        Func<string, ExternalServiceDelegate> getServices,
                                        Func<string, ExternalQueryDelegate> getQueries,
                                        TimeSpan timeout,
                                        ILogger logger)
        {
            service.CheckArgNull(nameof(service));
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));
            getServices.CheckArgNull(nameof(getServices));
            getQueries.CheckArgNull(nameof(getQueries));

            _service = service;
            _orchestrationManager = orchestrationManager;
            _getServices = getServices;
            _getQueries = getQueries;
            _logger = logger;
            _timeout = timeout;

            _orchestrationResolver = new NameVersionObjectManager<TaskOrchestration>();
            _activityResolver = new NameVersionObjectManager<TaskActivity>();
        }

        public async Task StartAsync(IRootStateMetadata metadata, CancellationToken token)
        {
            if (_worker != null)
            {
                throw new InvalidOperationException("Service already started.");
            }

            RegisterMetadata(metadata, token);

            _worker = new TaskHubWorker(_service, _orchestrationResolver, _activityResolver);

            AddTaskActivities(token);

            await _worker.StartAsync();
        }

        public Task StopAsync()
        {
            if (_worker == null)
            {
                throw new InvalidOperationException("Service not started.");
            }

            _orchestrationResolver.Clear();

            _activityResolver.Clear();

            return _worker.StopAsync();
        }

        private void RegisterStateChart(string instanceId, IRootStateMetadata metadata, CancellationToken token)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            metadata.CheckArgNull(nameof(metadata));

            // this is the durable activity for starting a child statechart from within its parent

            var createActivity = new CreateChildOrchestrationActivity(metadata, _orchestrationManager);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("startchildorchestration", instanceId, createActivity);

            _activityResolver.Add(activityCreator);

            // this is the orchestration that runs a statechart instance (parent or child)

            var orchestrator = new InterpreterOrchestration(metadata, token, _logger);

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

        private void RegisterMetadata(IRootStateMetadata metadata, CancellationToken token)
        {
            Debug.Assert(metadata != null);

            RegisterStateChart(metadata.UniqueId, metadata, token);

            metadata.RegisterStateChartInvokes((id, root) => RegisterStateChart(id, root, token));

            metadata.RegisterScripts(RegisterScript);
        }

        private void AddTaskActivities(CancellationToken token)
        {
            _worker.AddTaskActivities(typeof(GenerateGuidActivity));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendparentchildmessage",
                                                                               string.Empty,
                                                                               new SendParentChildMessageActivity(_orchestrationManager)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("waitforcompletion",
                                                                               string.Empty,
                                                                               new WaitForCompletionActivity(_orchestrationManager, _timeout / 2, token)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("logger", string.Empty, new LoggerActivity(_logger)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("query", string.Empty, new QueryActivity(_getQueries, token)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendmessage", string.Empty, new SendMessageActivity(_getServices, token)));
        }

        private class NameVersionObjectManager<T> : INameVersionObjectManager<T>
        {
            private readonly IDictionary<string, ObjectCreator<T>> _creators;

            public NameVersionObjectManager()
            {
                _creators = new Dictionary<string, ObjectCreator<T>>();
            }

            public void Clear()
            {
                _creators.Clear();
            }

            public void Add(ObjectCreator<T> creator)
            {
                var key = GetKey(creator.Name, creator.Version);

                lock (_creators)
                {
                    Debug.Assert(!_creators.ContainsKey(key));

                    _creators.TryAdd(key, creator);
                }
            }

            public T GetObject(string name, string version)
            {
                var key = GetKey(name, version);

                lock (_creators)
                {
                    if (_creators.TryGetValue(key, out ObjectCreator<T> creator))
                    {
                        return creator.Create();
                    }

                    return default;
                }
            }

            private string GetKey(string name, string version)
            {
                return $"{name}_{version}";
            }
        }
    }
}
