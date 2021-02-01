using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSM.Metadata.States;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DSM.Metadata
{
    public class StateConverter<TData> : JsonConverter<State<TData>>
    {
        public override void WriteJson(JsonWriter writer, [AllowNull] State<TData> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override State<TData> ReadJson(JsonReader reader,
                                              Type objectType,
                                              [AllowNull] State<TData> existingValue,
                                              bool hasExistingValue,
                                              JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            State<TData> state = null;

            switch (json["type"].Value<string>())
            {
                case "atomic":
                    state = json.ToObject<AtomicState<TData>>();
                    break;

                case "compound":
                    state = json.ToObject<CompoundState<TData>>();
                    break;

                case "parallel":
                    state = json.ToObject<ParallelState<TData>>();
                    break;

                case "final":
                    state = json.ToObject<FinalState<TData>>();
                    break;

                case "history":
                    state = json.ToObject<HistoryState<TData>>();
                    break;

                default:
                    Debug.Fail("Unexpected state type.");
                    break;
            }

            return state;
        }
    }
}
