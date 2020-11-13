using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
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
