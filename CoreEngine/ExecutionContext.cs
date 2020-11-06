using System;
using System.Collections.Generic;
using System.Text;
using CoreEngine.Model;
using CoreEngine.Model.States;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace CoreEngine
{
    public class ExecutionContext
    {
        private readonly ExpressionEvaluator _eval;
        private readonly Dictionary<string, object> _data;
        private readonly Queue<Event> _internalQueue;
        private readonly Queue<Event> _externalQueue;
        private readonly Dictionary<string, IEnumerable<State>> _historyValues;

        public ExecutionContext()
        {
            _data = new Dictionary<string, object>();
            _eval = new ExpressionEvaluator(this);
            _internalQueue = new Queue<Event>();
            _externalQueue = new Queue<Event>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
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

        internal DynamicDictionary ScriptData => new DynamicDictionary(_data);

        internal void SetDataValue(string key, object value)
        {
            _data[key] = value;
        }

        internal bool TryGet(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }

        internal Task<T> Eval<T>(string expression)
        {
            return _eval.Eval<T>(expression);
        }

        internal void EnqueueInternal(string eventName)
        {
            _internalQueue.Enqueue(new Event(eventName) { Type = EventType.Internal });
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
    }
}
