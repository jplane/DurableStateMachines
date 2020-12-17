using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;


namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class CancelMetadata : ExecutableContentMetadata, ICancelMetadata
    {
        internal CancelMetadata(JObject element)
            : base(element)
        {
        }

        public string SendId => _element.Property("sendid")?.Value.Value<string>() ?? string.Empty;

        public string SendIdExpr => _element.Property("sendidexpr")?.Value.Value<string>() ?? string.Empty;
    }
}
