using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using CoreEngine.Abstractions.Model.Execution.Metadata;
using CoreEngine.ModelProvider.Xml.DataManipulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class SendMetadata : ExecutableContentMetadata, ISendMetadata
    {
        public SendMetadata(XElement element)
            : base(element)
        {
        }

        public string Event => _element.Attribute("event")?.Value ?? string.Empty;

        public string EventExpression => _element.Attribute("eventexpr")?.Value ?? string.Empty;

        public string Target => _element.Attribute("target")?.Value ?? string.Empty;

        public string TargetExpression => _element.Attribute("targetexpr")?.Value ?? string.Empty;

        public string Type => _element.Attribute("type")?.Value ?? string.Empty;

        public string TypeExpression => _element.Attribute("typeexpr")?.Value ?? string.Empty;

        public string Id => _element.Attribute("id")?.Value ?? string.Empty;

        public string IdLocation => _element.Attribute("idlocation")?.Value ?? string.Empty;

        public string Delay => _element.Attribute("delay")?.Value ?? string.Empty;

        public string DelayExpression => _element.Attribute("delayexpr")?.Value ?? string.Empty;

        public IEnumerable<string> Namelist
        {
            get => (_element.Attribute("eventexpr")?.Value ?? string.Empty).Split(" ");
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
