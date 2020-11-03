using System;
using SCG=System.Collections.Generic;
using System.Text;

namespace CoreEngine
{
    internal class ExecutionContext
    {
        public ExecutionContext()
        {
        }

        public bool IsRunning { get; set; }

        public EventPublisher EventPublisher { get; } = new EventPublisher();

        public OrderedSet<State> Configuration { get; } = new OrderedSet<State>();

        public OrderedSet<State> StatesToInvoke { get; } = new OrderedSet<State>();

        public SCG.Queue<Event> InternalQueue { get; } = new SCG.Queue<Event>();

        public SCG.Queue<Event> ExternalQueue { get; } = new SCG.Queue<Event>();

        public SCG.Dictionary<string, List<State>> HistoryValue { get; } = new SCG.Dictionary<string, List<State>>();

        public DataModel DataModel { get; } = new DataModel();
    }
}
