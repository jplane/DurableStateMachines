using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DSM.Metadata.Actions;

namespace DSM.Metadata
{
    public class ActionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType &&
                   objectType.GetGenericTypeDefinition().IsSubclassOf(typeof(Actions.Action<>));
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
                case "assign":
                    actionType = typeof(Assign<>).MakeGenericType(genericArgumentType);
                    break;

                case "else":
                    actionType = typeof(Else<>).MakeGenericType(genericArgumentType);
                    break;

                case "elseif":
                    actionType = typeof(ElseIf<>).MakeGenericType(genericArgumentType);
                    break;

                case "foreach":
                    actionType = typeof(Foreach<>).MakeGenericType(genericArgumentType);
                    break;

                case "if":
                    actionType = typeof(If<>).MakeGenericType(genericArgumentType);
                    break;

                case "log":
                    actionType = typeof(Log<>).MakeGenericType(genericArgumentType);
                    break;

                case "logic":
                    actionType = typeof(Logic<>).MakeGenericType(genericArgumentType);
                    break;

                case "query":
                    actionType = typeof(Query<>).MakeGenericType(genericArgumentType);
                    break;

                case "raise":
                    actionType = typeof(Raise<>).MakeGenericType(genericArgumentType);
                    break;

                case "sendmessage":
                    actionType = typeof(SendMessage<>).MakeGenericType(genericArgumentType);
                    break;

                case "invokestatemachine":
                    actionType = typeof(InvokeStateMachine<>).MakeGenericType(genericArgumentType);
                    break;

                default:
                    Debug.Fail("Unexpected action type.");
                    break;
            }

            return json.ToObject(actionType);
        }
    }
}
