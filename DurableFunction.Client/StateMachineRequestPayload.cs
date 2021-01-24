using Newtonsoft.Json;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Metadata.States;
using System.Collections.Generic;

namespace StateChartsDotNet.DurableFunction.Client
{
    public class StateMachineRequestPayload
    {
        [JsonProperty("debug")]
        public DebuggerInfo DebugInfo { get; internal set; }

        [JsonProperty("args")]
        public Dictionary<string, object> Arguments { get; internal set; }

        [JsonProperty("statemachine")]
        public StateMachine StateMachineDefinition { get; internal set; }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
