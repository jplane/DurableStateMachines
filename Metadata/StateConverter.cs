using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Metadata.States;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StateChartsDotNet.Metadata
{
    public class StateConverter : JsonConverter<State>
    {
        public override void WriteJson(JsonWriter writer, [AllowNull] State value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override State ReadJson(JsonReader reader,
                                       Type objectType,
                                       [AllowNull] State existingValue,
                                       bool hasExistingValue,
                                       JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            State state = null;

            switch (json["type"].Value<string>())
            {
                case "atomic":
                    state = json.ToObject<AtomicState>();
                    break;

                case "compound":
                    state = json.ToObject<CompoundState>();
                    break;

                case "parallel":
                    state = json.ToObject<ParallelState>();
                    break;

                case "final":
                    state = json.ToObject<FinalState>();
                    break;

                case "history":
                    state = json.ToObject<HistoryState>();
                    break;

                default:
                    Debug.Fail("Unexpected state type.");
                    break;
            }

            return state;
        }
    }
}
