using Newtonsoft.Json;
using DSM.Metadata.States;
using System.Collections.Generic;
using DSM.Common.Observability;

namespace DSM.FunctionClient
{
    /// <summary>
    /// Input type for JSON-based state machine orchestrations.
    /// </summary>
    public sealed class StateMachineDefinitionPayload
    {
        /// <summary>
        /// Provides optional observability information for the state machine runtime.
        /// </summary>
        [JsonProperty("observables")]
        public Instruction[] Observables { get; set; }

        /// <summary>
        /// Initial execution data for the state machine runtime.
        /// </summary>
        [JsonProperty("input")]
        public Dictionary<string, object> Input { get; set; }

        /// <summary>
        /// The state machine definition.
        /// </summary>
        [JsonProperty("statemachine")]
        public StateMachine<Dictionary<string, object>> Definition { get; set; }

        [JsonProperty("parentstack")]
        internal string[] ParentInstanceStack { get; set; }
    }
}
