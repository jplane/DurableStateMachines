using CoreEngine.Abstractions.Model.Execution.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class ForeachMetadata : ExecutableContentMetadata, IForeachMetadata
    {
        public ForeachMetadata(XElement element)
            : base(element)
        {
        }

        public string ArrayExpression => _element.Attribute("array").Value;

        public string Item => _element.Attribute("item").Value;

        public string Index => _element.Attribute("index")?.Value ?? string.Empty;

        public Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements())
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return Task.FromResult(content.AsEnumerable());
        }
    }
}
