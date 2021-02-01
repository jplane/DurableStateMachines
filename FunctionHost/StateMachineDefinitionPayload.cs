using Newtonsoft.Json;
using DSM.Common.Debugger;
using DSM.Metadata.States;
using System.Collections.Generic;

namespace DSM.FunctionClient
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
