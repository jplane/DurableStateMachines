using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class DatamodelMetadata : IDatamodelMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<string> _uniqueId;

        public DatamodelMetadata(XElement element)
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

        public IEnumerable<IDataInitMetadata> GetData()
        {
            var nodes = _element.ScxmlElements("data");

            return nodes.Select(n => new DataInitMetadata(n)).Cast<IDataInitMetadata>();
        }
    }
}
