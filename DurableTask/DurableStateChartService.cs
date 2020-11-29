using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableTask
{
    public class DurableStateChartService
    {
        private readonly IRootStateMetadata _metadata;
        private readonly IOrchestrationService _service;
        private readonly ILogger _logger;
        private readonly Dictionary<string, IRootStateMetadata> _childMetadata;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;

        private TaskHubWorker _worker;

        public DurableStateChartService(IOrchestrationService service,
                                        IRootStateMetadata metadata,
                                        ILogger logger = null)
        {
            service.CheckArgNull(nameof(service));
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _service = service;
            _logger = logger;

            _childMetadata = new Dictionary<string, IRootStateMetadata>();
            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
        }

        public void ConfigureChildStateChart(IRootStateMetadata statechart)
        {
            statechart.CheckArgNull(nameof(statechart));

            _childMetadata[statechart.Id] = statechart;
        }

        public void ConfigureExternalQuery(string id, ExternalQueryDelegate handler)
        {
            id.CheckArgNull(nameof(id));
            handler.CheckArgNull(nameof(handler));

            _externalQueries[id] = handler;
        }

        public void ConfigureExternalService(string id, ExternalServiceDelegate handler)
        {
            id.CheckArgNull(nameof(id));
            handler.CheckArgNull(nameof(handler));

            _externalServices[id] = handler;
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

            ensureOrchestrationRegistration(_metadata.Id,
                                            () => new InterpreterOrchestration(_metadata,
                                                                               ensureActivityRegistration,
                                                                               ensureOrchestrationRegistration,
                                                                               _childMetadata,
                                                                               _externalServices,
                                                                               _externalQueries,
                                                                               (IOrchestrationServiceClient)_service,
                                                                               _logger));

            _worker = new TaskHubWorker(_service, orchestrationResolver, activityResolver);

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
