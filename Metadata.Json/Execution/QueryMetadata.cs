using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class QueryMetadata : ExecutableContentMetadata, IQueryMetadata
    {
        internal QueryMetadata(JObject element)
            : base(element)
        {
        }

        public string ActivityType => _element.Property("activity")?.Value.Value<string>();

        public string ResultLocation => _element.Property("resultlocation")?.Value.Value<string>();

        public string ConfigurationType => _element.Property("configtype")?.Value.Value<string>();

        public IQueryConfiguration Configuration
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.ConfigurationType))
                {
                    throw new InvalidOperationException("Missing configuration type for query metadata.");
                }

                Func<Type, bool> filter = t =>
                    typeof(IQueryConfiguration).IsAssignableFrom(t) &&
                           string.Compare(t.FullName, this.ConfigurationType, true, CultureInfo.InvariantCulture) == 0;

                var type = AppDomain.CurrentDomain.GetAssemblies()
                                                  .SelectMany(asm => asm.GetTypes())
                                                  .SingleOrDefault(filter);

                if (type == null)
                {
                    throw new InvalidOperationException("Unable to resolve specified configuration type for query metadata.");
                }

                return (IQueryConfiguration) _element.Property("config")?.Value.ToObject(type);
            }
        }

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            var finalizeElements = _element.Property("finalize")?.Value.Values<JObject>() ?? Enumerable.Empty<JObject>();

            foreach (var node in finalizeElements)
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }
    }
}
