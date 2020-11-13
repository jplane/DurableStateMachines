using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class ContentMetadata : IContentMetadata
    {
        private readonly XElement _element;

        public ContentMetadata(XElement element)
        {
            _element = element;
        }

        public string Expression => _element.Attribute("expr")?.Value ?? string.Empty;

        public string Body => _element.Value ?? string.Empty;
    }
}
