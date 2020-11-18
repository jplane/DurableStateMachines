using System;
using System.Collections.Generic;
using StateChartsDotNet.CoreEngine.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine.Abstractions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StateChartsDotNet.CoreEngine
{
    public class ExecutionContext
    {
        private readonly Dictionary<string, object> _data;
        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<Message> _internalMessages;
        private readonly Queue<Message> _externalMessages;
        private readonly Set<State> _configuration;
        private readonly Set<State> _statesToInvoke;

        private ILogger _logger;

        public ExecutionContext()
        {
            _data = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalMessages = new Queue<Message>();
            _externalMessages = new Queue<Message>();
            _configuration = new Set<State>();
            _statesToInvoke = new Set<State>();
        }

        internal virtual Task Init(RootState root)
        {
            this.SetDataValue("_sessionid", Guid.NewGuid().ToString("D"));
            this.SetDataValue("_name", root.Name);

            return Task.CompletedTask;
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

        public Task SendAsync(string message, params object[] dataPairs)
        {
            var msg = new Message(message)
            {
                Type = MessageType.External
            };

            for (var idx = 0; idx < dataPairs.Length; idx += 2)
            {
                msg[(string) dataPairs[idx]] = dataPairs[idx + 1];
            }

            return SendAsync(msg);
        }

        public virtual Task SendAsync(Message message)
        {
            lock (_externalMessages)
            {
                _externalMessages.Enqueue(message);
            }

            return Task.CompletedTask;
        }

        internal virtual async Task<Message> DequeueExternal()
        {
            bool TryGet(out Message evt)
            {
                lock (_externalMessages)
                {
                    return _externalMessages.TryDequeue(out evt);
                }
            }

            Message msg;

            while (! TryGet(out msg))
            {
                await Task.Delay(1000);
            }

            _data["_event"] = msg;

            return msg;
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

            _internalMessages.Enqueue(evt);
        }

        internal void EnqueueCommunicationError(Exception ex)
        {
            var evt = new Message("error.communication")
            {
                Type = MessageType.Platform
            };

            evt["exception"] = ex;

            _internalMessages.Enqueue(evt);

            _logger.LogError("Communication error", ex);
        }

        internal void EnqueueExecutionError(Exception ex)
        {
            var evt = new Message("error.execution")
            {
                Type = MessageType.Platform
            };

            evt["exception"] = ex;

            _internalMessages.Enqueue(evt);

            _logger.LogError("Execution error", ex);
        }

        internal bool HasInternalMessages => _internalMessages.Count > 0;

        internal Message DequeueInternal()
        {
            if (_internalMessages.TryDequeue(out Message evt))
            {
                _data["_event"] = evt;
            }

            return evt;
        }

        internal Set<State> Configuration => _configuration;

        internal Set<State> StatesToInvoke => _statesToInvoke;

        internal bool TryGetHistoryValue(string key, out IEnumerable<State> value)
        {
            return _historyValues.TryGetValue(key, out value);
        }

        internal void StoreHistoryValue(string key, Func<State, bool> predicate)
        {
            predicate.CheckArgNull(nameof(predicate));

            _historyValues[key] = this.Configuration.Where(predicate).ToArray();
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
