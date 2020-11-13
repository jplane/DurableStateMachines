using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class ParamMetadata : IParamMetadata
    {
        private readonly XElement _element;

        public ParamMetadata(XElement element)
        {
            _element = element;
        }

        public string Name => _element.Attribute("name").Value;

        public string Location => _element.Attribute("location")?.Value ?? string.Empty;

        public string Expression => _element.Attribute("expr")?.Value ?? string.Empty;
    }
}
