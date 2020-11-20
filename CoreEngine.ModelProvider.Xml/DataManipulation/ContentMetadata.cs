using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class ContentMetadata : IContentMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<string> _uniqueId;

        public ContentMetadata(XElement element)
        {
            _element = element;

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public string UniqueId => _uniqueId.Value;

        public bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public string Expression => _element.Attribute("expr")?.Value ?? string.Empty;

        public string Body => _element.Value ?? string.Empty;
    }
}
