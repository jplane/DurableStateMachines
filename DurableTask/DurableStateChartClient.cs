using DurableTask.Core;
using DurableTask.Core.Serializing;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableTask
{
    public class DurableStateChartClient
    {
        private readonly TaskHubClient _client;
        private readonly string _stateChartName;
        private readonly string _instanceId;
        private readonly Dictionary<string, object> _data;
        private OrchestrationInstance _instance;
        private OrchestrationState _result;

        public DurableStateChartClient(IOrchestrationServiceClient serviceClient,
                                       string stateChartName,
                                       string instanceId = null)
        {
            serviceClient.CheckArgNull(nameof(serviceClient));
            stateChartName.CheckArgNull(nameof(stateChartName));

            _client = new TaskHubClient(serviceClient);
            _stateChartName = stateChartName;
            _instanceId = instanceId;
            _data = new Dictionary<string, object>();
        }

        internal static async Task SendMessageToParent(IOrchestrationServiceClient orchestrationClient,
                                                       string parentInstanceId,
                                                       string messageName,
                                                       string invokeId,
                                                       object content,
                                                       IReadOnlyDictionary<string, object> parameters)
        {
            orchestrationClient.CheckArgNull(nameof(orchestrationClient));
            parentInstanceId.CheckArgNull(nameof(parentInstanceId));
            messageName.CheckArgNull(nameof(messageName));
            invokeId.CheckArgNull(nameof(invokeId));
            parameters.CheckArgNull(nameof(parameters));

            var client = new TaskHubClient(orchestrationClient);

            var state = await client.GetOrchestrationStateAsync(parentInstanceId);

            Debug.Assert(state != null);

            var msg = new ChildStateChartResponseMessage(messageName)
            {
                CorrelationId = invokeId,
                Content = content,
                Parameters = parameters
            };

            await client.RaiseEventAsync(state.OrchestrationInstance, messageName, msg);
        }

        private bool IsRunning => _instance != null;

        public string State => _result?.Status ?? "NotStarted";

        public object this[string key]
        {
            get
            {
                lock (_data)
                {
                    return _data[key];
                }
            }

            set
            {
                if (IsRunning)
                {
                    throw new InvalidOperationException("Cannot set execution state while the state machine is running.");
                }

                lock (_data)
                {
                    _data[key] = value;
                }
            }
        }

        public async Task InitAsync()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("Statechart already started.");
            }

            _result = null;

            if (!string.IsNullOrWhiteSpace(_instanceId))
            {
                var state = await _client.GetOrchestrationStateAsync(_instanceId);

                if (state != null)
                {
                    _instance = state.OrchestrationInstance;
                }
            }

            if (_instance == null)
            {
                _instance = await _client.CreateOrchestrationInstanceAsync("statechart",
                                                                           _stateChartName,
                                                                           _instanceId ?? _stateChartName,
                                                                           _data);
            }
        }

        public async Task WaitForCompletionAsync(TimeSpan timeout)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Statechart not yet started.");
            }

            _result = await _client.WaitForOrchestrationAsync(_instance, timeout);

            _instance = null;

            lock (_data)
            {
                var dataconverter = new JsonDataConverter();

                var output = dataconverter.Deserialize<Dictionary<string, object>>(_result.Output);

                Debug.Assert(output != null);

                _data.Clear();

                foreach (var pair in output)
                {
                    _data[pair.Key] = pair.Value;
                }
            }
        }

        public Task StopAsync()
        {
            return SendMessageAsync(new ExternalMessage("cancel"));
        }

        public Task SendMessageAsync(ExternalMessage message)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("Statechart not yet started.");
            }

            return _client.RaiseEventAsync(_instance, message.Name, message);
        }
    }
}
