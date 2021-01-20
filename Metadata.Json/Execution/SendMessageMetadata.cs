using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        public string ConfigurationType => _element.Property("configtype")?.Value.Value<string>();

        public ISendMessageConfiguration Configuration
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.ConfigurationType))
                {
                    throw new InvalidOperationException("Missing configuration type for send message metadata.");
                }

                Func<Type, bool> filter = t =>
                    typeof(ISendMessageConfiguration).IsAssignableFrom(t) &&
                           string.Compare(t.FullName, this.ConfigurationType, true, CultureInfo.InvariantCulture) == 0;

                var type = AppDomain.CurrentDomain.GetAssemblies()
                                                  .SelectMany(asm => asm.GetTypes())
                                                  .SingleOrDefault(filter);

                if (type == null)
                {
                    throw new InvalidOperationException("Unable to resolve specified configuration type for send message metadata.");
                }

                return (ISendMessageConfiguration) _element.Property("config")?.Value.ToObject(type);
            }
        }
    }
}
