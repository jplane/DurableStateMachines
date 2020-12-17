using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;


namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class RaiseMetadata : ExecutableContentMetadata, IRaiseMetadata
    {
        internal RaiseMetadata(JObject element)
            : base(element)
        {
        }

        public string MessageName => _element.Property("event").Value.Value<string>();
    }
}
