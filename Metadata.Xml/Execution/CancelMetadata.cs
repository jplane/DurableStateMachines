using StateChartsDotNet.Common.Model.Execution;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public class CancelMetadata : ExecutableContentMetadata, ICancelMetadata
    {
        internal CancelMetadata(XElement element)
            : base(element)
        {
        }

        public string SendId => _element.Attribute("sendid")?.Value ?? string.Empty;

        public string SendIdExpr => _element.Attribute("sendidexpr")?.Value ?? string.Empty;
    }
}
