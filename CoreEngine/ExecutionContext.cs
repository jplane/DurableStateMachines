using System;
using SCG=System.Collections.Generic;
using System.Text;
using CoreEngine.Model;
using CoreEngine.Model.States;
using System.Threading;

namespace CoreEngine
{
    public class ExecutionContext
    {
        private readonly ExpressionEvaluator _eval;
        private readonly ExecutionState _state;
        private readonly SCG.Queue<Event> _internalQueue;
        private readonly SCG.Queue<Event> _externalQueue;

        public ExecutionContext()
        {
            _state = new ExecutionState();
            _eval = new ExpressionEvaluator(_state);
            _internalQueue = new SCG.Queue<Event>();
            _externalQueue = new SCG.Queue<Event>();
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
                Thread.Sleep(1000);
            }

            if (!evt.IsCancel)
            {
                _state["_event"] = evt;
            }

            return evt;
        }

        internal OrderedSet<State> Configuration { get; } = new OrderedSet<State>();

        internal OrderedSet<State> StatesToInvoke { get; } = new OrderedSet<State>();

        internal SCG.Dictionary<string, List<State>> HistoryValue { get; } = new SCG.Dictionary<string, List<State>>();
    }
}
