using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;

namespace StateChartsDotNet.DurableFunction.Client
{
    public class StateMachineRequestPayload
    {
        [JsonProperty("debug")]
        public DebuggerInfo DebugInfo { get; internal set; }

        [JsonProperty("args")]
        public Dictionary<string, object> Arguments { get; internal set; }

        [JsonProperty("format")]
        public StateMachineDefinitionFormat Format { get; internal set; }

        [JsonProperty("statemachine")]
        public JToken StateMachineDefinition { get; internal set; }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal IStateChartMetadata GetMetadata()
        {
            if (this.StateMachineDefinition == null)
            {
                return null;
            }

            IStateChartMetadata result = null;

            switch (this.Format)
            {
                case StateMachineDefinitionFormat.Json:
                    result = new Metadata.Json.States.StateChart((JObject) this.StateMachineDefinition);
                    break;

                case StateMachineDefinitionFormat.Fluent:
                    result = Metadata.Fluent.States.StateChart.Deserialize(this.StateMachineDefinition.Value<string>());
                    break;
            }

            return result;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StateMachineDefinitionFormat
    {
        Json = 1,
        Fluent
    }
}
