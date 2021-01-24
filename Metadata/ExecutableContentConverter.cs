using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Metadata.Execution;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace StateChartsDotNet.Metadata
{
    public class ExecutableContentConverter : JsonConverter<ExecutableContent>
    {
        public override void WriteJson(JsonWriter writer, [AllowNull] ExecutableContent value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override ExecutableContent ReadJson(JsonReader reader,
                                                   Type objectType,
                                                   [AllowNull] ExecutableContent existingValue,
                                                   bool hasExistingValue,
                                                   JsonSerializer serializer)
        {
            var json = JObject.Load(reader);

            ExecutableContent content = null;

            switch (json["type"].Value<string>())
            {
                case "assign":
                    content = json.ToObject<Assign>();
                    break;

                case "else":
                    content = json.ToObject<Else>();
                    break;

                case "elseif":
                    content = json.ToObject<ElseIf>();
                    break;

                case "foreach":
                    content = json.ToObject<Foreach>();
                    break;

                case "if":
                    content = json.ToObject<If>();
                    break;

                case "log":
                    content = json.ToObject<Log>();
                    break;

                case "query":
                    content = json.ToObject<Query>();
                    break;

                case "raise":
                    content = json.ToObject<Raise>();
                    break;

                case "script":
                    content = json.ToObject<Script>();
                    break;

                case "sendmessage":
                    content = json.ToObject<SendMessage>();
                    break;

                default:
                    Debug.Fail("Unexpected executable content type.");
                    break;
            }

            return content;
        }
    }
}
