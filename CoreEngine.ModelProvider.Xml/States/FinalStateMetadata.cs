using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
{
    public class FinalStateMetadata : StateMetadata, IFinalStateMetadata
    {
        public FinalStateMetadata(XElement element)
            : base(element)
        {
        }

        public Task<IContentMetadata> GetContent()
        {
            var node = _element.ScxmlElement("content");

            return Task.FromResult(node == null ? null : (IContentMetadata)new ContentMetadata(node));
        }

        public Task<IEnumerable<IParamMetadata>> GetParams()
        {
            var nodes = _element.ScxmlElements("param");

            return Task.FromResult(nodes.Select(n => new ParamMetadata(n)).Cast<IParamMetadata>());
        }
    }
}
