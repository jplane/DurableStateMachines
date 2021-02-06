using Newtonsoft.Json;
using DSM.FunctionClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class DebugHook : StateMachineObserver
    {
        public DebugHook()
        {
        }

        private void OnDebugEvent(IDictionary<string, object> data)
        {
            foreach (var key in data.Keys)
            {
                Console.WriteLine($"{key} = {JsonConvert.SerializeObject(data[key])}");
            }

            Console.WriteLine();
        }

        protected override Task OnEnterStateMachine(IDictionary<string, object> data)
        {
            OnDebugEvent(data);
            return base.OnEnterStateMachine(data);
        }

        protected override Task OnExitStateMachine(IDictionary<string, object> data)
        {
            OnDebugEvent(data);
            return base.OnExitStateMachine(data);
        }

        protected override Task OnEnterState(IDictionary<string, object> data)
        {
            OnDebugEvent(data);
            return base.OnEnterState(data);
        }

        protected override Task OnExitState(IDictionary<string, object> data)
        {
            OnDebugEvent(data);
            return base.OnExitState(data);
        }

        protected override Task OnMakeTransition(IDictionary<string, object> data)
        {
            OnDebugEvent(data);
            return base.OnMakeTransition(data);
        }

        protected override Task OnBeforeAction(IDictionary<string, object> data)
        {
            OnDebugEvent(data);
            return base.OnBeforeAction(data);
        }

        protected override Task OnAfterAction(IDictionary<string, object> data)
        {
            OnDebugEvent(data);
            return base.OnAfterAction(data);
        }
    }
}
