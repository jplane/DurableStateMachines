using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.DataManipulation
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
