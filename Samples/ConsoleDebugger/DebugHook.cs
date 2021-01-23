using StateChartsDotNet.DurableFunction.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDebugger
{
    internal class DebugHook : DebugHandler
    {
        public DebugHook()
        {
        }

        protected override Task OnEnterStateMachine(IDictionary<string, object> data)
        {
            Console.WriteLine($"Enter state machine: {data["id"]}");
            return base.OnEnterStateMachine(data);
        }

        protected override Task OnExitStateMachine(IDictionary<string, object> data)
        {
            Console.WriteLine($"Exit state machine: {data["id"]}");
            return base.OnExitStateMachine(data);
        }

        protected override Task OnEnterState(IDictionary<string, object> data)
        {
            Console.WriteLine($"Enter state: {data["id"]}");
            return base.OnEnterState(data);
        }

        protected override Task OnExitState(IDictionary<string, object> data)
        {
            Console.WriteLine($"Exit state: {data["id"]}");
            return base.OnExitState(data);
        }
    }
}
