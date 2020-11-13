using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class DatamodelMetadata : IDatamodelMetadata
    {
        private readonly XElement _element;

        public DatamodelMetadata(XElement element)
        {
            _element = element;
        }

        public Task<IEnumerable<IDataInitMetadata>> GetData()
        {
            var nodes = _element.ScxmlElements("data");

            return Task.FromResult(nodes.Select(n => new DataInitMetadata(n)).Cast<IDataInitMetadata>());
        }
    }
}
