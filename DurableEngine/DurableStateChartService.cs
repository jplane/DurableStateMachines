using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class DurableStateChartService
    {
        private readonly IOrchestrationService _service;
        private readonly ExecutionContext _context;

        private TaskHubWorker _worker;

        public DurableStateChartService(IOrchestrationService service, ExecutionContext context)
        {
            service.CheckArgNull(nameof(service));
            context.CheckArgNull(nameof(context));

            _service = service;
            _context = context;
        }

        public async Task StartAsync()
        {
            if (_worker != null)
            {
                throw new InvalidOperationException("Service already started.");
            }

            var orchestrationResolver = new NameVersionObjectManager<TaskOrchestration>();

            var activityResolver = new NameVersionObjectManager<TaskActivity>();

            Action<string, Func<TaskActivity>> ensureActivityRegistration = (uniqueId, func) =>
            {
                uniqueId.CheckArgNull(nameof(uniqueId));
                func.CheckArgNull(nameof(func));

                var creator = new NameValueObjectCreator<TaskActivity>(uniqueId, string.Empty, func());

                activityResolver.Add(creator);
            };

            Action<string, Func<InterpreterOrchestration>> ensureOrchestrationRegistration = (name, func) =>
            {
                name.CheckArgNull(nameof(name));
                func.CheckArgNull(nameof(func));

                var creator = new NameValueObjectCreator<TaskOrchestration>("statechart", name, func());

                orchestrationResolver.Add(creator);
            };

            ensureOrchestrationRegistration(_context.Metadata.Id,
                                            () => new InterpreterOrchestration(_context.Metadata,
                                                                               ensureActivityRegistration,
                                                                               ensureOrchestrationRegistration,
                                                                               _context.ChildMetadata,
                                                                               _context.ExternalServices,
                                                                               _context.ExternalQueries,
                                                                               _context.Logger));

            _worker = new TaskHubWorker(_service, orchestrationResolver, activityResolver);

            _worker.AddTaskActivities(typeof(GenerateGuidActivity));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("logger", string.Empty, new LoggerActivity(_context.Logger)));

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

                    _creators[key] = creator;
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
