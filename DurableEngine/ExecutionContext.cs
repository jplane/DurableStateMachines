using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    public class ExecutionContext : IExecutionContext
    {
        private readonly IRootStateMetadata _metadata;
        private readonly ILogger _logger;

        private Dictionary<string, object> _data;
        private Func<ExternalMessage, Task> _sendMessageHandler;

        public ExecutionContext(IRootStateMetadata metadata, ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _logger = logger;
            _data = new Dictionary<string, object>();
        }

        internal IRootStateMetadata Metadata => _metadata;

        internal ILogger Logger => _logger;

        internal Func<ExternalMessage, Task> SendMessageHandler
        {
            set => _sendMessageHandler = value;
        }

        internal Dictionary<string, object> Data
        {
            get => _data;
            set => _data = value;
        }

        public object this[string key]
        {
            get => _data[key]; 
            set => _data[key] = value;
        }

        public bool IsRunning => _sendMessageHandler != null;

        public Task SendAsync(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (!this.IsRunning)
            {
                throw new InvalidOperationException("Cannot send messages until the state machine is running.");
            }

            Debug.Assert(_sendMessageHandler != null);

            return _sendMessageHandler(message);
        }

        public Task SendAsync(string message,
                              object content = null,
                              IReadOnlyDictionary<string, object> parameters = null)
        {
            message.CheckArgNull(nameof(message));

            var msg = new ExternalMessage(message)
            {
                Content = content,
                Parameters = parameters
            };

            return SendAsync(msg);
        }

        public Task StopAsync()
        {
            return SendAsync("cancel");
        }
    }
}
