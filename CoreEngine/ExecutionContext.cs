using System;
using System.Collections.Generic;
using System.Text;
using CoreEngine.Model;
using CoreEngine.Model.States;
using System.Threading;
using System.Linq;

namespace CoreEngine
{
    public class ExecutionContext
    {
        private readonly ExpressionEvaluator _eval;
        private readonly ExecutionState _state;
        private readonly Queue<Event> _internalQueue;
        private readonly Queue<Event> _externalQueue;
        private readonly Dictionary<string, IEnumerable<State>> _historyValues;

        public ExecutionContext()
        {
            _state = new ExecutionState();
            _eval = new ExpressionEvaluator(_state);
            _internalQueue = new Queue<Event>();
            _externalQueue = new Queue<Event>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
        }

        public bool IsRunning { get; internal set; }

        public object this[string key]
        {
            get { return _state[key]; }

            set
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot set execution state while the state machine is running.");
                }

                _state[key] = value;
            }
        }

        internal void SetStateValue(string key, object value)
        {
            _state[key] = value;
        }

        internal bool TryGet(string key, out object value)
        {
            return _state.TryGetValue(key, out value);
        }

        internal T Eval<T>(string expression)
        {
            return (T) _eval.Eval(expression);
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
                _state["_event"] = evt;
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

        internal Event DequeueExternal()
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
                Thread.Sleep(100);
            }

            if (!evt.IsCancel)
            {
                _state["_event"] = evt;
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
            _historyValues[key] = states.ToArray();
        }
    }
}
