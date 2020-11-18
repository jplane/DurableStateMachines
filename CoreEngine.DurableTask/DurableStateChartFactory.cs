using CoreEngine.DurableTask;
using DurableTask.Core;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.DurableTask
{
    public static class DurableStateChartFactory
    {
        public static async Task<TaskHubWorker> Create(IModelMetadata metadata, IOrchestrationService service)
        {
            metadata.CheckArgNull(nameof(metadata));
            service.CheckArgNull(nameof(service));

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

            var executionContext = new DurableExecutionContext(metadata, ensureActivityRegistration);

            var worker = new TaskHubWorker(service, orchestrationResolver, activityResolver);

            worker.AddTaskOrchestrations(new NameValueObjectCreator<TaskOrchestration>("statechart",
                                                                                       string.Empty,
                                                                                       new InterpreterOrchestration(executionContext)));

            await service.CreateIfNotExistsAsync();

            return await worker.StartAsync();
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
