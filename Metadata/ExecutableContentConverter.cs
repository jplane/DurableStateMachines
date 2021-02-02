using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DSM.Metadata.Execution;

namespace DSM.Metadata
{
    public class ExecutableContentConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType &&
                   objectType.GetGenericTypeDefinition().IsSubclassOf(typeof(ExecutableContent<>));
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

            Type actionType = null;

            switch (json["type"].Value<string>())
            {
                case "assign`1":
                    actionType = typeof(Assign<>).MakeGenericType(genericArgumentType);
                    break;

                case "else`1":
                    actionType = typeof(Else<>).MakeGenericType(genericArgumentType);
                    break;

                case "elseif`1":
                    actionType = typeof(ElseIf<>).MakeGenericType(genericArgumentType);
                    break;

                case "foreach`1":
                    actionType = typeof(Foreach<>).MakeGenericType(genericArgumentType);
                    break;

                case "if`1":
                    actionType = typeof(If<>).MakeGenericType(genericArgumentType);
                    break;

                case "log`1":
                    actionType = typeof(Log<>).MakeGenericType(genericArgumentType);
                    break;

                case "logic`1":
                    actionType = typeof(Logic<>).MakeGenericType(genericArgumentType);
                    break;

                case "query`1":
                    actionType = typeof(Query<>).MakeGenericType(genericArgumentType);
                    break;

                case "raise`1":
                    actionType = typeof(Raise<>).MakeGenericType(genericArgumentType);
                    break;

                case "sendmessage`1":
                    actionType = typeof(SendMessage<>).MakeGenericType(genericArgumentType);
                    break;

                default:
                    Debug.Fail("Unexpected action type.");
                    break;
            }

            return json.ToObject(actionType);
        }
    }
}
