using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.ModelProvider.Xml.DataManipulation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public class ServiceMetadata : IServiceMetadata
    {
        private readonly XElement _element;

        public ServiceMetadata(XElement element)
        {
            _element = element;
        }

        public bool Autoforward
        {
            get
            {
                var afattr = _element.Attribute("autoforward");

                if (afattr != null && bool.TryParse(afattr.Value, out bool result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
        }

        public string Type => _element.Attribute("type")?.Value ?? string.Empty;
        
        public string TypeExpression => _element.Attribute("typeexpr")?.Value ?? string.Empty;

        public string Id => _element.Attribute("id")?.Value ?? string.Empty;

        public string IdLocation => _element.Attribute("idlocation")?.Value ?? string.Empty;

        public string Source => _element.Attribute("src")?.Value ?? string.Empty;

        public string SourceExpression => _element.Attribute("srcexpr")?.Value ?? string.Empty;

        public IEnumerable<string> Namelist
        {
            get => (_element.Attribute("typeexpr")?.Value ?? string.Empty).Split(" ");
        }

        public Task<IContentMetadata> GetContent()
        {
            var node = _element.ScxmlElement("content");

            return Task.FromResult(node == null ? null : (IContentMetadata) new ContentMetadata(node));
        }

        public Task<IFinalizeMetadata> GetFinalize()
        {
            var node = _element.ScxmlElement("finalize");

            return Task.FromResult(node == null ? null : (IFinalizeMetadata) new FinalizeMetadata(node));
        }

        public Task<IEnumerable<IParamMetadata>> GetParams()
        {
            var nodes = _element.ScxmlElements("param");

            return Task.FromResult(nodes.Select(n => new ParamMetadata(n)).Cast<IParamMetadata>());
        }
    }
}
