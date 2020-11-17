using System;
using System.Collections.Generic;
using StateChartsDotNet.CoreEngine.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine.Abstractions;

namespace StateChartsDotNet.CoreEngine
{
    public class ExecutionContext
    {
        private readonly Dictionary<string, object> _data;
        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<Message> _messages = new Queue<Message>();
        private ILogger _logger;

        public ExecutionContext()
        {
            _data = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
        }

        public bool IsRunning { get; internal set; }

        public ILogger Logger
        {
            set => _logger = value;
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

        internal void EnqueueInternal(string eventName, params object[] dataPairs)
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

            _messages.Enqueue(evt);
        }

        internal void EnqueueCommunicationError(Exception ex)
        {
            var evt = new Message("error.communication")
            {
                Type = MessageType.Platform
            };

            evt["exception"] = ex;

            _messages.Enqueue(evt);

            _logger.LogError("Communication error", ex);
        }

        internal void EnqueueExecutionError(Exception ex)
        {
            var evt = new Message("error.execution")
            {
                Type = MessageType.Platform
            };

            evt["exception"] = ex;

            _messages.Enqueue(evt);

            _logger.LogError("Execution error", ex);
        }

        internal bool HasInternalMessages => _messages.Count > 0;

        internal Message DequeueInternal()
        {
            if (_messages.TryDequeue(out Message evt))
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
