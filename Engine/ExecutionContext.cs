using System;
using System.Collections.Generic;
using StateChartsDotNet.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using System.Runtime.CompilerServices;
using Nito.AsyncEx;
using StateChartsDotNet.Common.Model.States;

[assembly: InternalsVisibleTo("StateChartsDotNet.DurableTask")]

namespace StateChartsDotNet
{
    public class ExecutionContext
    {
        protected readonly Dictionary<string, object> _data;
        protected readonly ILogger _logger;

        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<Message> _internalMessages;
        private readonly AsyncProducerConsumerQueue<Message> _externalMessages;
        private readonly Set<State> _configuration;
        private readonly Set<State> _statesToInvoke;
        private readonly RootState _root;

        public ExecutionContext(IRootStateMetadata metadata, ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _root = new RootState(metadata);
            _logger = logger;

            _data = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalMessages = new Queue<Message>();
            _externalMessages = new AsyncProducerConsumerQueue<Message>();
            _configuration = new Set<State>();
            _statesToInvoke = new Set<State>();
        }

        public bool IsRunning { get; internal set; }

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

        public void Send(string message, params object[] dataPairs)
        {
            var msg = new Message(message)
            {
                Type = MessageType.External
            };

            for (var idx = 0; idx < dataPairs.Length; idx += 2)
            {
                msg[(string) dataPairs[idx]] = dataPairs[idx + 1];
            }

            Send(msg);
        }

        public virtual void Send(Message message)
        {
            _externalMessages.Enqueue(message);
        }

        internal virtual Task ExecuteContent(string uniqueId, Func<ExecutionContext, Task> func)
        {
            func.CheckArgNull(nameof(func));

            return func(this);
        }

        internal virtual Task Init()
        {
            this["_sessionid"] = Guid.NewGuid().ToString("D");

            this["_name"] = this.Root.Name;

            return Task.CompletedTask;
        }

        internal RootState Root => _root;

        internal async Task<Message> DequeueExternal()
        {
            var msg = await _externalMessages.DequeueAsync();

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

        internal virtual Task LogDebug(string message)
        {
            _logger?.LogDebug(message);

            return Task.CompletedTask;
        }

        internal virtual Task LogInformation(string message)
        {
            _logger?.LogInformation(message);

            return Task.CompletedTask;
        }
    }
}
