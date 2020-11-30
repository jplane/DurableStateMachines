using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    public class ExecutionContext : IExecutionContext
    {
        private readonly IRootStateMetadata _metadata;
        private readonly ILogger _logger;
        private readonly Dictionary<string, IRootStateMetadata> _childMetadata;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;

        private Dictionary<string, object> _data;
        private Func<ExternalMessage, Task> _sendMessageHandler;

        public ExecutionContext(IRootStateMetadata metadata, ILogger logger = null)
        {
            Debug.Assert(metadata != null);

            _metadata = metadata;
            _logger = logger;

            _childMetadata = new Dictionary<string, IRootStateMetadata>();
            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _data = new Dictionary<string, object>();
        }

        internal IRootStateMetadata Metadata => _metadata;

        internal ILogger Logger => _logger;

        internal IReadOnlyDictionary<string, IRootStateMetadata> ChildMetadata => _childMetadata;

        internal IReadOnlyDictionary<string, ExternalServiceDelegate> ExternalServices => _externalServices;

        internal IReadOnlyDictionary<string, ExternalQueryDelegate> ExternalQueries => _externalQueries;

        internal IReadOnlyDictionary<string, object> Data
        {
            get => _data;
            set => _data = new Dictionary<string, object>(value);
        }

        internal Func<ExternalMessage, Task> SendMessageHandler
        {
            set => _sendMessageHandler = value;
        }

        public object this[string key]
        {
            get { return _data[key]; }

            set
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot set execution state while the state machine is running.");
                }

                _data[key] = value;
            }
        }

        public bool IsRunning => _sendMessageHandler != null;

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

        public Task SendAsync(string message,
                              object content = null,
                              IReadOnlyDictionary<string, object> parameters = null)
        {
            if (!this.IsRunning)
            {
                throw new InvalidOperationException("Cannot send messages until the state machine is running.");
            }

            Debug.Assert(_sendMessageHandler != null);

            var msg = new ExternalMessage(message)
            {
                Content = content,
                Parameters = parameters
            };

            return _sendMessageHandler(msg);
        }

        public Task StopAsync()
        {
            return SendAsync("cancel");
        }
    }
}
