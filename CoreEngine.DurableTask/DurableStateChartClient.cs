using DurableTask.Core;
using StateChartsDotNet.CoreEngine.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.DurableTask
{
    public class DurableStateChartClient
    {
        private readonly TaskHubClient _client;
        private OrchestrationInstance _instance;
        private OrchestrationState _result;

        public DurableStateChartClient(IOrchestrationServiceClient serviceClient)
        {
            serviceClient.CheckArgNull(nameof(serviceClient));

            _client = new TaskHubClient(serviceClient);
        }

        public string State => _result?.Status ?? "NotStarted";

        public async Task StartAsync()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Statechart already started.");
            }

            _result = null;

            _instance = await _client.CreateOrchestrationInstanceAsync("statechart", "", null);
        }

        public async Task WaitForCompletionAsync(TimeSpan timeout)
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("Statechart not yet started.");
            }

            _result = await _client.WaitForOrchestrationAsync(_instance, timeout);
        }

        public Task SendMessageAsync(Message message)
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("Statechart not yet started.");
            }

            return _client.RaiseEventAsync(_instance, message.Name, message);
        }
    }
}
