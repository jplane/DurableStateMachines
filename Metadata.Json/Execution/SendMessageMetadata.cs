using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;
using System;

namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class SendMessageMetadata : ExecutableContentMetadata, ISendMessageMetadata
    {
        internal SendMessageMetadata(JObject element)
            : base(element)
        {
        }

        public string Id => _element.Property("id")?.Value.Value<string>();

        public string IdLocation => _element.Property("idlocation")?.Value.Value<string>();

        public TimeSpan Delay => TimeSpan.Parse(_element.Property("delay")?.Value.Value<string>() ?? "00:00:00");

        public string ActivityType => _element.Property("activity")?.Value.Value<string>();

        public JObject Config => _element.Property("config")?.Value.Value<JObject>();
    }
}
