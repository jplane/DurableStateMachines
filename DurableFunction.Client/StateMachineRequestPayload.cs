using Newtonsoft.Json;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Metadata.States;
using System.Collections.Generic;

namespace StateChartsDotNet.DurableFunction.Client
{
    public class StateMachineRequestPayload<TData>
    {
        [JsonProperty("debug")]
        public DebuggerInfo DebugInfo { get; internal set; }

        [JsonProperty("args")]
        public TData Arguments { get; internal set; }

        [JsonProperty("statemachineid")]
        public string StateMachineIdentifier { get; internal set; }

        [JsonProperty("ischildstatemachine")]
        public bool IsChildStateMachine { get; internal set; }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
