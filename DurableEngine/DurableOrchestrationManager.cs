using DurableTask.Core;
using DurableTask.Core.Serializing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public interface IOrchestrationManager
    {
        Task StartAsync();

        Task StopAsync(bool forced = false);

        Task RegisterAsync(string metadataId, IStateChartMetadata metadata);

        Task StartInstanceAsync(string metadataId, string instanceId, IDictionary<string, object> data);

        Task<IReadOnlyDictionary<string, object>> WaitForInstanceAsync(string instanceId);

        Task SendMessageAsync(string instanceId, ExternalMessage message);

        Task<OrchestrationState> GetInstanceAsync(string instanceId);
    }

    internal class DurableOrchestrationManager : IOrchestrationManager
    {
        private readonly List<string> _statecharts;
        private readonly Dictionary<string, IInvokeStateChartMetadata> _childInvokes;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancelToken;
        private readonly TimeSpan _timeout;
        private readonly AsyncLock _lock;
        private readonly IOrchestrationStorage _storage;
        private readonly IOrchestrationService _service;
        private readonly string _callbackUri;

        private TaskHubWorker _worker;
        private TaskHubClient _client;

        public DurableOrchestrationManager(IOrchestrationService service,
                                           IOrchestrationStorage storage,
                                           TimeSpan timeout,
                                           CancellationToken cancelToken,
                                           string callbackUri = null,
                                           ILogger logger = null)
        {
            service.CheckArgNull(nameof(service));
            storage.CheckArgNull(nameof(storage));

            if (!(service is IOrchestrationServiceClient))
            {
                throw new ArgumentException("Expecting orchestration service to implement both client and service interfaces.");
            }

            _service = service;
            _storage = storage;
            _timeout = timeout;
            _cancelToken = cancelToken;
            _callbackUri = callbackUri;
            _logger = logger;

            _lock = new AsyncLock();
            _statecharts = new List<string>();
            _childInvokes = new Dictionary<string, IInvokeStateChartMetadata>();

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", HttpService.PostAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", HttpService.GetAsync);
        }

        private bool Started => _worker != null;

        public async Task StartAsync()
        {
            using (await _lock.LockAsync())
            {
                if (Started)
                {
                    return;
                }

                _worker = new TaskHubWorker(_service);
                _client = new TaskHubClient((IOrchestrationServiceClient) _service);

                AddTaskActivities();

                await _storage.DeserializeAsync(async (metadataId, stream, metadataType) =>
                {
                    Debug.Assert(!string.IsNullOrWhiteSpace(metadataId));
                    Debug.Assert(stream != null);
                    Debug.Assert(!string.IsNullOrWhiteSpace(metadataType));

                    Type targetType = null;

                    switch (metadataType)
                    {
                        case "fluent":
                            targetType = Type.GetType("StateChartsDotNet.Metadata.Fluent.States.StateChart, StateChartsDotNet.Metadata.Fluent");
                            break;

                        case "json":
                            targetType = Type.GetType("StateChartsDotNet.Metadata.Json.States.StateChart, StateChartsDotNet.Metadata.Json");
                            break;

                        case "xml":
                            targetType = Type.GetType("StateChartsDotNet.Metadata.Xml.States.StateChart, StateChartsDotNet.Metadata.Xml");
                            break;
                    }

                    Debug.Assert(targetType != null);

                    var method = targetType.GetMethod("DeserializeAsync",
                                                      BindingFlags.Public | BindingFlags.Static);

                    Debug.Assert(method != null);

                    var deserializer =
                            (Func<Stream, CancellationToken, Task<IStateChartMetadata>>) Delegate.CreateDelegate(typeof(Func<Stream, CancellationToken, Task<IStateChartMetadata>>), method);

                    Debug.Assert(deserializer != null);

                    var metadata = await deserializer(stream, _cancelToken);

                    Debug.Assert(metadata != null);

                    Register(metadataId, metadata);
                });

                await _worker.StartAsync();
            }
        }

        public async Task StopAsync(bool forced = false)
        {
            using (await _lock.LockAsync())
            {
                if (! Started)
                {
                    return;
                }

                await _worker.StopAsync(forced);

                _childInvokes.Clear();
                _statecharts.Clear();

                _client = null;
                _worker = null;
            }
        }

        public async Task RegisterAsync(string metadataId, IStateChartMetadata metadata)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            metadata.CheckArgNull(nameof(metadata));

            using (await _lock.LockAsync())
            {
                if (! Started)
                {
                    throw new InvalidOperationException("Service not started.");
                }

                if (_statecharts.Contains(metadataId))
                {
                    return;
                }

                using var stream = new MemoryStream();

                var metadataType = await metadata.SerializeAsync(stream, _cancelToken);

                Debug.Assert(!string.IsNullOrWhiteSpace(metadataType));

                stream.Position = 0;

                await _storage.SerializeAsync(metadataId, stream, metadataType);

                Register(metadataId, metadata);
            }
        }

        public async Task StartInstanceAsync(string metadataId,
                                             string instanceId,
                                             IDictionary<string, object> data)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            instanceId.CheckArgNull(nameof(instanceId));
            data.CheckArgNull(nameof(data));

            if (! Started)
            {
                throw new InvalidOperationException("Service not started.");
            }

            if (_childInvokes.TryGetValue(metadataId, out IInvokeStateChartMetadata invokeMetadata) &&
                invokeMetadata.ExecutionMode == ChildStateChartExecutionMode.Remote)
            {
                if (string.IsNullOrWhiteSpace(_callbackUri))
                {
                    throw new InvalidOperationException("Host is not configured for remote child statechart invocation.");
                }

                await HttpService.StartRemoteChildStatechartAsync(_callbackUri, invokeMetadata, metadataId, instanceId, data, _cancelToken);
            }
            else
            {
                await _client.CreateOrchestrationInstanceAsync("statechart", metadataId, instanceId, data);
            }
        }

        public async Task<IReadOnlyDictionary<string, object>> WaitForInstanceAsync(string instanceId)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            if (! Started)
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

            if (! Started)
            {
                throw new InvalidOperationException("Service not started.");
            }

            return await _client.GetOrchestrationStateAsync(instanceId);
        }

        public async Task SendMessageAsync(string instanceId, ExternalMessage message)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            message.CheckArgNull(nameof(message));

            if (! Started)
            {
                throw new InvalidOperationException("Service not started.");
            }

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            await _client.RaiseEventAsync(instance, message.Name, message);
        }

        private void Register(string metadataId, IStateChartMetadata metadata)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(metadataId));
            Debug.Assert(metadata != null);

            RegisterStateChart(metadataId, metadata);

            metadata.RegisterStateChartInvokes(RegisterStateChartInvoke, metadataId);
        }

        private void RegisterStateChart(string metadataId, IStateChartMetadata metadata)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            metadata.CheckArgNull(nameof(metadata));

            Debug.Assert(_worker != null);
            Debug.Assert(! _statecharts.Contains(metadataId));

            _statecharts.Add(metadataId);

            var orchestrator = new InterpreterOrchestration(metadata, _cancelToken, _logger);

            var orchestrationCreator = new NameValueObjectCreator<TaskOrchestration>("statechart", metadataId, orchestrator);

            _worker.AddTaskOrchestrations(orchestrationCreator);
        }

        private void RegisterStateChartInvoke(string metadataId, IInvokeStateChartMetadata metadata)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            metadata.CheckArgNull(nameof(metadata));

            Debug.Assert(! _childInvokes.ContainsKey(metadataId));

            _childInvokes.Add(metadataId, metadata);

            var stateChartMetadata = metadata.GetRoot();

            Debug.Assert(stateChartMetadata != null);

            RegisterStateChart(metadataId, stateChartMetadata);
        }

        private void AddTaskActivities()
        {
            Debug.Assert(_worker != null);

            _worker.AddTaskActivities(typeof(GenerateGuidActivity));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("sendparentchildmessage",
                                                                               string.Empty,
                                                                               new SendParentChildMessageActivity(this)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("waitforcompletion",
                                                                               string.Empty,
                                                                               new WaitForCompletionActivity(this)));

            _worker.AddTaskActivities(new NameValueObjectCreator<TaskActivity>("startchildorchestration",
                                                                               string.Empty,
                                                                               new CreateChildOrchestrationActivity(this)));

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
