using Newtonsoft.Json;
using DSM.Common.Debugger;
using DSM.Metadata.States;
using System.Collections.Generic;

namespace DSM.FunctionClient
{
    /// <summary>
    /// Input type for JSON-based state machine orchestrations.
    /// </summary>
    public class StateMachineDefinitionPayload
    {
        /// <summary>
        /// Provides optional debugging information for the state machine runtime.
        /// </summary>
        [JsonProperty("debug")]
        public DebuggerInfo DebugInfo { get; set; }

        /// <summary>
        /// Initial execution data for the state machine runtime.
        /// </summary>
        [JsonProperty("input")]
        public Dictionary<string, object> Input { get; set; }

        /// <summary>
        /// The state machine definition.
        /// </summary>
        [JsonProperty("statemachineid")]
        public StateMachine<Dictionary<string, object>> Definition { get; set; }

        [JsonProperty("ischildstatemachine")]
        internal bool IsChildStateMachine { get; set; }
    }
}
