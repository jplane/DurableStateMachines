using Newtonsoft.Json;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Metadata.States;
using System.Collections.Generic;

namespace StateChartsDotNet.DurableFunctionClient
{
    public class StateMachineDefinitionPayload
    {
        [JsonProperty("debug")]
        public DebuggerInfo DebugInfo { get; set; }

        [JsonProperty("input")]
        public Dictionary<string, object> Input { get; set; }

        [JsonProperty("statemachineid")]
        public StateMachine<Dictionary<string, object>> Definition { get; set; }

        [JsonProperty("ischildstatemachine")]
        public bool IsChildStateMachine { get; set; }
    }
}
