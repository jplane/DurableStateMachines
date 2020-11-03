using System;
using SCG=System.Collections.Generic;
using System.Text;
using CoreEngine.Model;

namespace CoreEngine
{
    internal class ExecutionContext
    {
        public ExecutionContext()
        {
        }

        public bool IsRunning { get; set; }

        public EventPublisher EventPublisher { get; } = new EventPublisher();

        public OrderedSet<_State> Configuration { get; } = new OrderedSet<_State>();

        public OrderedSet<_State> StatesToInvoke { get; } = new OrderedSet<_State>();

        public SCG.Queue<Event> InternalQueue { get; } = new SCG.Queue<Event>();

        public SCG.Queue<Event> ExternalQueue { get; } = new SCG.Queue<Event>();

        public SCG.Dictionary<string, List<_State>> HistoryValue { get; } = new SCG.Dictionary<string, List<_State>>();

        public ExecutionState ExecutionState { get; } = new ExecutionState();
    }
}
