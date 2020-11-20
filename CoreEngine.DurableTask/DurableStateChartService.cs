using CoreEngine.DurableTask;
using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.DurableTask
{
    public class DurableStateChartService
    {
        private readonly IRootStateMetadata _metadata;
        private readonly IOrchestrationService _service;
        private readonly ILogger _logger;

        private TaskHubWorker _worker;

        public DurableStateChartService(IRootStateMetadata metadata,
                                        IOrchestrationService service,
                                        ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));
            service.CheckArgNull(nameof(service));

            _metadata = metadata;
            _service = service;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            if (_worker != null)
            {
                throw new InvalidOperationException("Service already started.");
            }

            var orchestrationResolver = new NameVersionObjectManager<TaskOrchestration>();

            var activityResolver = new NameVersionObjectManager<TaskActivity>();

            Action<string, ExecutionContext, Func<ExecutionContext, Task>> ensureActivityRegistration = (uniqueId, ec, func) =>
            {
                uniqueId.CheckArgNull(nameof(uniqueId));
                ec.CheckArgNull(nameof(ec));
                func.CheckArgNull(nameof(func));

                var creator = new NameValueObjectCreator<TaskActivity>(uniqueId,
                                                                       string.Empty,
                                                                       new ExecutableContentActivity(func, ec));

                activityResolver.Add(creator);
            };

            _worker = new TaskHubWorker(_service, orchestrationResolver, activityResolver);

            _worker.AddTaskOrchestrations(new NameValueObjectCreator<TaskOrchestration>("statechart",
                                                                                        string.Empty,
                                                                                        new InterpreterOrchestration(_metadata,
                                                                                                                     ensureActivityRegistration,
                                                                                                                     _logger)));

            _worker.AddTaskActivities(typeof(GenerateGuidActivity));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("logger", string.Empty, new LoggerActivity(_logger)));

            await _worker.StartAsync();
        }

        public Task StopAsync()
        {
            if (_worker == null)
            {
                throw new InvalidOperationException("Service not started.");
            }

            return _worker.StopAsync();
        }

        private class NameVersionObjectManager<T> : INameVersionObjectManager<T>
        {
            private readonly IDictionary<string, ObjectCreator<T>> _creators;

            public NameVersionObjectManager()
            {
                _creators = new Dictionary<string, ObjectCreator<T>>();
            }

            public void Add(ObjectCreator<T> creator)
            {
                lock (_creators)
                {
                    var key = GetKey(creator.Name, creator.Version);

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
