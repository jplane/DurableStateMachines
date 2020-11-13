using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
{
    public class CancelMetadata : ExecutableContentMetadata, ICancelMetadata
    {
        public CancelMetadata(XElement element)
            : base(element)
        {
        }

        public string SendId => _element.Attribute("sendid")?.Value ?? string.Empty;

        public string SendIdExpr => _element.Attribute("sendidexpr")?.Value ?? string.Empty;
    }
}
