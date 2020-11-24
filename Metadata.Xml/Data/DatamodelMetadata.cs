using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Data
{
    public class DatamodelMetadata : IDatamodelMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<string> _uniqueId;

        internal DatamodelMetadata(XElement element)
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
