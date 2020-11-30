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
    internal class DurableStateChartClient
    {
        private readonly TaskHubClient _client;
        private readonly ExecutionContext _context;

        private OrchestrationInstance _instance;
        private OrchestrationState _result;

        public DurableStateChartClient(IOrchestrationServiceClient serviceClient, ExecutionContext context)
        {
            serviceClient.CheckArgNull(nameof(serviceClient));
            context.CheckArgNull(nameof(context));

            _client = new TaskHubClient(serviceClient);
            _context = context;
        }

        public object this[string key]
        {
            get
            {
                lock (_context)
                {
                    return _context[key];
                }
            }

            set
            {
                lock (_context)
                {
                    _context[key] = value;
                }
            }
        }

        public async Task InitAsync()
        {
            if (_context.IsRunning)
            {
                throw new InvalidOperationException("Statechart already started.");
            }

            _result = null;

            _instance = await _client.CreateOrchestrationInstanceAsync("statechart", _context.Metadata.Id, _context.Data);

            _context.SendMessageHandler = message => _client.RaiseEventAsync(_instance, message.Name, message);
        }

        public async Task WaitForCompletionAsync(TimeSpan timeout)
        {
            if (! _context.IsRunning)
            {
                throw new InvalidOperationException("Statechart not yet started.");
            }

            _result = await _client.WaitForOrchestrationAsync(_instance, timeout);

            _instance = null;

            _context.SendMessageHandler = null;

            lock (_context)
            {
                var dataconverter = new JsonDataConverter();

                var output = dataconverter.Deserialize<Dictionary<string, object>>(_result.Output);

                Debug.Assert(output != null);

                _context.Data = output;
            }
        }
    }
}
