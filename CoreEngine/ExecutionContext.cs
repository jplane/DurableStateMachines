using System;
using System.Collections.Generic;
using StateChartsDotNet.CoreEngine.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine.Abstractions;
using StateChartsDotNet.CoreEngine.Abstractions.MessageTransports;
using StateChartsDotNet.CoreEngine.MessageTransports;

namespace StateChartsDotNet.CoreEngine
{
    public class ExecutionContext
    {
        private readonly Dictionary<string, object> _data;
        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private ILogger _logger;
        private Lazy<IMessageTransport> _internalTransport;
        private Lazy<IMessageTransport> _externalTransport;

        public ExecutionContext()
        {
            _data = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalTransport = new Lazy<IMessageTransport>(() => new InMemoryQueueTransport());
            _externalTransport = new Lazy<IMessageTransport>(() => new InMemoryQueueTransport());
        }

        public bool IsRunning { get; internal set; }

        public ILogger Logger
        {
            set => _logger = value;
        }

        public IMessageTransport InternalTransport
        {
            internal get => _internalTransport.Value;
            
            set
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot set internal message transport while the state machine is running.");
                }

                _internalTransport = new Lazy<IMessageTransport>(() => value);
            }
        }

        public IMessageTransport ExternalTransport
        {
            internal get => _externalTransport.Value;

            set
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot set external message transport while the state machine is running.");
                }

                _externalTransport = new Lazy<IMessageTransport>(() => value);
            }
        }

        public Task SendAsync(string eventName, params object[] dataPairs)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(eventName));
            Debug.Assert(dataPairs.Length % 2 == 0);

            var evt = new Message(eventName)
            {
                Type = MessageType.External
            };

            for (var idx = 0; idx < dataPairs.Length; idx += 2)
            {
                evt[(string)dataPairs[idx]] = dataPairs[idx + 1];
            }

            return _externalTransport.Value.SendAsync(evt);
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

        internal DynamicDictionary ScriptData => new DynamicDictionary(_data);

        internal void SetDataValue(string key, object value)
        {
            _data[key] = value;
        }

        internal bool TryGet(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }

        internal Task EnqueueInternal(string eventName, params object[] dataPairs)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(eventName));
            Debug.Assert(dataPairs.Length % 2 == 0);

            var evt = new Message(eventName)
            {
                Type = MessageType.Internal
            };

            for (var idx = 0; idx < dataPairs.Length; idx+=2)
            {
                evt[(string) dataPairs[idx]] = dataPairs[idx + 1];
            }

            return _internalTransport.Value.SendAsync(evt);
        }

        internal async Task EnqueueCommunicationError(Exception ex)
        {
            var evt = new Message("error.communication")
            {
                Type = MessageType.Platform
            };

            evt["exception"] = ex;

            await _internalTransport.Value.SendAsync(evt);

            _logger.LogError("Communication error", ex);
        }

        internal async Task EnqueueExecutionError(Exception ex)
        {
            var evt = new Message("error.execution")
            {
                Type = MessageType.Platform
            };

            evt["exception"] = ex;

            await _internalTransport.Value.SendAsync(evt);

            _logger.LogError("Execution error", ex);
        }

        internal Task<bool> HasInternalMessages => _internalTransport.Value.HasMessageAsync();

        internal async Task<Message> DequeueInternal()
        {
            if (await _internalTransport.Value.TryReceiveAsync(out Message evt))
            {
                _data["_event"] = evt;
            }

            return evt;
        }

        internal async Task<Message> DequeueExternal()
        {
            Task<bool> Dequeue(out Message evt)
            {
                return _externalTransport.Value.TryReceiveAsync(out evt);
            }

            Message evt;

            while (! await Dequeue(out evt))
            {
                await Task.Delay(100);
            }

            if (!evt.IsCancel)
            {
                _data["_event"] = evt;
            }

            return evt;
        }

        internal Set<State> Configuration { get; } = new Set<State>();

        internal Set<State> StatesToInvoke { get; } = new Set<State>();

        internal bool TryGetHistoryValue(string key, out IEnumerable<State> value)
        {
            return _historyValues.TryGetValue(key, out value);
        }

        internal void StoreHistoryValue(string key, IEnumerable<State> states)
        {
            states.CheckArgNull(nameof(states));

            _historyValues[key] = states.ToArray();
        }

        internal void LogDebug(string message)
        {
            _logger?.LogDebug(message);
        }

        internal void LogInformation(string message)
        {
            _logger?.LogInformation(message);
        }
    }
}
