using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class DonedataMetadata : IDonedataMetadata
    {
        private readonly XElement _element;

        public DonedataMetadata(XElement element)
        {
            _element = element;
        }

        public Task<IContentMetadata> GetContent()
        {
            var node = _element.ScxmlElement("content");

            return Task.FromResult(node == null ? null : (IContentMetadata) new ContentMetadata(node));
        }

        public Task<IEnumerable<IParamMetadata>> GetParams()
        {
            var nodes = _element.ScxmlElements("param");

            return Task.FromResult(nodes.Select(n => new ParamMetadata(n)).Cast<IParamMetadata>());
        }
    }
}
