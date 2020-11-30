using DurableTask.Core;
using DurableTask.Core.Serializing;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    public class DurableStateChartClient
    {
        private readonly TaskHubClient _client;
        private readonly string _stateChartName;
        private readonly Dictionary<string, object> _data;
        private OrchestrationInstance _instance;
        private OrchestrationState _result;

        public DurableStateChartClient(IOrchestrationServiceClient serviceClient, string stateChartName)
        {
            serviceClient.CheckArgNull(nameof(serviceClient));
            stateChartName.CheckArgNull(nameof(stateChartName));

            _client = new TaskHubClient(serviceClient);
            _stateChartName = stateChartName;
            _data = new Dictionary<string, object>();
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

            _instance = await _client.CreateOrchestrationInstanceAsync("statechart", _stateChartName, _data);
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
