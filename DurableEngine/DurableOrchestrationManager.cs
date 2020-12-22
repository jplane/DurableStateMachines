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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal interface IOrchestrationManager
    {
        Task StartAsync();

        Task StopAsync(bool forced = false);

        Task RegisterAsync(IStateChartMetadata metadata);

        Task StartInstanceAsync(string metadataId, string instanceId, IDictionary<string, object> data);

        Task<IReadOnlyDictionary<string, object>> WaitForInstanceAsync(string instanceId);

        Task SendMessageAsync(string instanceId, ExternalMessage message);

        Task<OrchestrationState> GetInstanceAsync(string instanceId);
    }

    internal class DurableOrchestrationManager : IOrchestrationManager
    {
        private readonly List<string> _statecharts;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly NameVersionObjectManager<TaskOrchestration> _orchestrationResolver;
        private readonly NameVersionObjectManager<TaskActivity> _activityResolver;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancelToken;
        private readonly TimeSpan _timeout;
        private readonly AsyncLock _lock;
        private readonly TaskHubWorker _worker;
        private readonly IOrchestrationStorage _storage;
        private readonly TaskHubClient _client;

        private bool _started;

        public DurableOrchestrationManager(IOrchestrationService service,
                                           IOrchestrationStorage storage,
                                           TimeSpan timeout,
                                           CancellationToken cancelToken,
                                           ILogger logger = null)
        {
            service.CheckArgNull(nameof(service));
            storage.CheckArgNull(nameof(storage));

            if (!(service is IOrchestrationServiceClient))
            {
                throw new ArgumentException("Expecting orchestration service to implement both client and service interfaces.");
            }

            _storage = storage;
            _timeout = timeout;
            _cancelToken = cancelToken;
            _logger = logger;

            _started = false;

            _lock = new AsyncLock();
            _statecharts = new List<string>();

            _orchestrationResolver = new NameVersionObjectManager<TaskOrchestration>();
            _activityResolver = new NameVersionObjectManager<TaskActivity>();

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", HttpService.PostAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", HttpService.GetAsync);

            _worker = new TaskHubWorker(service, _orchestrationResolver, _activityResolver);
            _client = new TaskHubClient((IOrchestrationServiceClient) service);
        }

        public async Task StartAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_started)
                {
                    return;
                }

                AddTaskActivities();

                await _storage.DeserializeAsync(async (metadataId, deserializationType, stream) =>
                {
                    Debug.Assert(!string.IsNullOrWhiteSpace(metadataId));
                    Debug.Assert(!string.IsNullOrWhiteSpace(deserializationType));
                    Debug.Assert(stream != null);

                    var type = Type.GetType(deserializationType);

                    Debug.Assert(type != null);

                    var method = type.GetMethod("DeserializeAsync", BindingFlags.Public | BindingFlags.Static);

                    Debug.Assert(method != null);

                    var deserializer =
                            (Func<Stream, Task<IStateChartMetadata>>) Delegate.CreateDelegate(typeof(Func<Stream, Task<IStateChartMetadata>>), method);

                    Debug.Assert(deserializer != null);

                    var metadata = await deserializer(stream);

                    Debug.Assert(metadata != null);

                    Debug.Assert(metadata.MetadataId == metadataId);

                    Register(metadata);
                });

                await _worker.StartAsync();

                _started = true;
            }
        }

        public async Task StopAsync(bool forced = false)
        {
            using (await _lock.LockAsync())
            {
                if (!_started)
                {
                    return;
                }

                await _worker.StopAsync(forced);

                _orchestrationResolver.Clear();
                _activityResolver.Clear();
                _statecharts.Clear();

                _started = false;
            }
        }

        public async Task RegisterAsync(IStateChartMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            using (await _lock.LockAsync())
            {
                if (_statecharts.Contains(metadata.MetadataId))
                {
                    return;
                }

                using (var stream = new MemoryStream())
                {
                    var deserializationType = await metadata.SerializeAsync(stream, _cancelToken);

                    Debug.Assert(!string.IsNullOrWhiteSpace(deserializationType));

                    stream.Position = 0;

                    await _storage.SerializeAsync(metadata.MetadataId, deserializationType, stream);
                }

                Register(metadata);
            }
        }

        public async Task StartInstanceAsync(string metadataId,
                                             string instanceId,
                                             IDictionary<string, object> data)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            instanceId.CheckArgNull(nameof(instanceId));
            data.CheckArgNull(nameof(data));

            if (!_started)
            {
                throw new InvalidOperationException("Service not started.");
            }

            await _client.CreateOrchestrationInstanceAsync("statechart", metadataId, instanceId, data);
        }

        public async Task<IReadOnlyDictionary<string, object>> WaitForInstanceAsync(string instanceId)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            if (!_started)
            {
                throw new InvalidOperationException("Service not started.");
            }

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var result = await _client.WaitForOrchestrationAsync(instance, _timeout, _cancelToken);

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

        public async Task<OrchestrationState> GetInstanceAsync(string instanceId)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            if (!_started)
            {
                throw new InvalidOperationException("Service not started.");
            }

            return await _client.GetOrchestrationStateAsync(instanceId);
        }

        public async Task SendMessageAsync(string instanceId, ExternalMessage message)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            message.CheckArgNull(nameof(message));

            if (!_started)
            {
                throw new InvalidOperationException("Service not started.");
            }

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            await _client.RaiseEventAsync(instance, message.Name, message);
        }

        private void Register(IStateChartMetadata metadata)
        {
            Debug.Assert(metadata != null);

            RegisterStateChart(metadata.MetadataId, metadata);

            metadata.RegisterStateChartInvokes((id, root, inline) => RegisterStateChart(id, root, inline), metadata.MetadataId);

            metadata.RegisterScripts((id, metadata) => RegisterScript(id, metadata), metadata.MetadataId);
        }

        private void RegisterStateChart(string metadataId, IStateChartMetadata metadata, bool executeInline = false)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            metadata.CheckArgNull(nameof(metadata));

            Debug.Assert(! _statecharts.Contains(metadataId));

            _statecharts.Add(metadataId);

            // this is the durable activity for starting a child statechart from within its parent

            var createActivity = new CreateChildOrchestrationActivity(metadataId, this);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("startchildorchestration", metadataId, createActivity);

            _activityResolver.Add(activityCreator);

            // this is the orchestration that runs a statechart instance (parent or child)

            var orchestrator = new InterpreterOrchestration(metadata, _cancelToken, executeInline, _logger);

            var orchestrationCreator = new NameValueObjectCreator<TaskOrchestration>("statechart", metadataId, orchestrator);

            _orchestrationResolver.Add(orchestrationCreator);
        }

        private void RegisterScript(string metadataId, IScriptMetadata metadata)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            metadata.CheckArgNull(nameof(metadata));

            var scriptActivity = new ExecuteScriptActivity(metadata);

            var activityCreator = new NameValueObjectCreator<TaskActivity>("script", metadataId, scriptActivity);

            _activityResolver.Add(activityCreator);
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
