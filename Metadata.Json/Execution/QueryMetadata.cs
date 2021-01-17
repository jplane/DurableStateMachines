using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;
using System.Collections.Generic;
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

        public JObject Config => _element.Property("config")?.Value.Value<JObject>();

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
