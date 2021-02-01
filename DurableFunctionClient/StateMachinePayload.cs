using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Common.Model.States;
using System.Diagnostics;

namespace StateChartsDotNet.DurableFunctionClient
{
    public class StateMachinePayload
    {
        [JsonProperty("debug")]
        public DebuggerInfo DebugInfo { get; set; }

        [JsonProperty("input")]
        public object Input { get; set; }

        [JsonProperty("statemachineid")]
        public string StateMachineIdentifier { get; set; }

        [JsonProperty("ischildstatemachine")]
        public bool IsChildStateMachine { get; set; }

        internal object DeserializeInput(IStateChartMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            if (this.Input == null)
            {
                return null;
            }

            Debug.Assert(this.Input is JObject);

            var metadataType = metadata.GetType();

            Debug.Assert(metadataType.IsGenericType);
            
            var genericArgs = metadataType.GetGenericArguments();

            Debug.Assert(genericArgs.Length == 1);

            return ((JObject) this.Input).ToObject(genericArgs[0]);
        }
    }
}
