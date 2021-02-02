using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSM.Metadata.States;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DSM.Metadata
{
    public class StateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType &&
                   objectType.GetGenericTypeDefinition().IsSubclassOf(typeof(State<>));
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                        [AllowNull] object existingValue,
                                        JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            Debug.Assert(objectType.IsGenericType);

            var genericArgumentType = objectType.GetGenericArguments().Single();

            Type stateType = null;

            switch (json["type"].Value<string>())
            {
                case "atomic":
                    stateType = typeof(AtomicState<>).MakeGenericType(genericArgumentType);
                    break;

                case "compound":
                    stateType = typeof(CompoundState<>).MakeGenericType(genericArgumentType);
                    break;

                case "parallel":
                    stateType = typeof(ParallelState<>).MakeGenericType(genericArgumentType);
                    break;

                case "final":
                    stateType = typeof(FinalState<>).MakeGenericType(genericArgumentType);
                    break;

                case "history":
                    stateType = typeof(HistoryState<>).MakeGenericType(genericArgumentType);
                    break;

                default:
                    Debug.Fail("Unexpected state type.");
                    break;
            }

            return json.ToObject(stateType);
        }
    }
}
