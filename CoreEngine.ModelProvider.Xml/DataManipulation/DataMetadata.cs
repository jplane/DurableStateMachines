using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class DataMetadata : IDataMetadata
    {
        private readonly XElement _element;

        public DataMetadata(XElement element)
        {
            _element = element;
        }

        public string Id => _element.Attribute("id").Value;

        public string Source => _element.Attribute("src")?.Value ?? string.Empty;

        public string Expression => _element.Attribute("expr")?.Value ?? string.Empty;

        public string Body => _element.Value ?? string.Empty;
    }
}
