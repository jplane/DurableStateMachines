using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Metadata.Execution;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StateChartsDotNet.Metadata
{
    public class ExecutableContentConverter<TData> : JsonConverter<ExecutableContent<TData>>
    {
        public override void WriteJson(JsonWriter writer, [AllowNull] ExecutableContent<TData> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override ExecutableContent<TData> ReadJson(JsonReader reader,
                                                          Type objectType,
                                                          [AllowNull] ExecutableContent<TData> existingValue,
                                                          bool hasExistingValue,
                                                          JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            ExecutableContent<TData> content = null;

            switch (json["type"].Value<string>())
            {
                case "assign":
                    content = json.ToObject<Assign<TData>>();
                    break;

                case "else":
                    content = json.ToObject<Else<TData>>();
                    break;

                case "elseif":
                    content = json.ToObject<ElseIf<TData>>();
                    break;

                case "foreach":
                    content = json.ToObject<Foreach<TData>>();
                    break;

                case "if":
                    content = json.ToObject<If<TData>>();
                    break;

                case "log":
                    content = json.ToObject<Log<TData>>();
                    break;

                case "query":
                    content = json.ToObject<Query<TData>>();
                    break;

                case "raise":
                    content = json.ToObject<Raise<TData>>();
                    break;

                case "script":
                    content = json.ToObject<Script<TData>>();
                    break;

                case "sendmessage":
                    content = json.ToObject<SendMessage<TData>>();
                    break;

                default:
                    Debug.Fail("Unexpected executable content type.");
                    break;
            }

            return content;
        }
    }
}
