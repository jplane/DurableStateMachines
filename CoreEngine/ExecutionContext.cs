using System;
using System.Collections.Generic;
using StateChartsDotNet.CoreEngine.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace StateChartsDotNet.CoreEngine
{
    public class ExecutionContext
    {
        private readonly Dictionary<string, object> _data;
        private readonly Queue<Event> _internalQueue;
        private readonly Queue<Event> _externalQueue;
        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private ILogger _logger;

        public ExecutionContext()
        {
            _data = new Dictionary<string, object>();
            _internalQueue = new Queue<Event>();
            _externalQueue = new Queue<Event>();
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

            var evt = new Event(eventName)
            {
                Type = EventType.Internal
            };

            for (var idx = 0; idx < dataPairs.Length; idx+=2)
            {
                evt[(string) dataPairs[idx]] = dataPairs[idx + 1];
            }

            _internalQueue.Enqueue(evt);
        }

        internal void EnqueueCommunicationError(Exception ex)
        {
            var evt = new Event("error.communication")
            {
                Type = EventType.Platform
            };

            evt["exception"] = ex;

            _internalQueue.Enqueue(evt);

            _logger.LogError("Communication error", ex);
        }

        internal void EnqueueExecutionError(Exception ex)
        {
            var evt = new Event("error.execution")
            {
                Type = EventType.Platform
            };

            evt["exception"] = ex;

            _internalQueue.Enqueue(evt);

            _logger.LogError("Execution error", ex);
        }

        internal bool HasInternalEvents => _internalQueue.Count > 0;

        internal Event DequeueInternal()
        {
            if (_internalQueue.TryDequeue(out Event evt))
            {
                _data["_event"] = evt;
            }

            return evt;
        }

        public void Enqueue(string eventName)
        {
            lock (_externalQueue)
            {
                _externalQueue.Enqueue(new Event(eventName) { Type = EventType.External });
            }
        }

        internal async Task<Event> DequeueExternal()
        {
            bool Dequeue(out Event evt)
            {
                lock (_externalQueue)
                {
                    return _externalQueue.TryDequeue(out evt);
                }
            }

            Event evt;

            while (!Dequeue(out evt))
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
