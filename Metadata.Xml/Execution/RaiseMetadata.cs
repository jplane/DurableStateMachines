using StateChartsDotNet.Common.Model.Execution;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public class RaiseMetadata : ExecutableContentMetadata, IRaiseMetadata
    {
        public RaiseMetadata(XElement element)
            : base(element)
        {
        }

        public string MessageName => _element.Attribute("event").Value;
    }
}
