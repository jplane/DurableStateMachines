using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;


namespace StateChartsDotNet.Metadata.Json.Data
{
    public class DatamodelMetadata : IDatamodelMetadata
    {
        private readonly JProperty _element;
        private readonly string _metadataId;

        internal DatamodelMetadata(JProperty element)
        {
            _element = element;

            _metadataId = element.GetUniqueElementPath();
        }

        public string MetadataId => _metadataId;

        public bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public IEnumerable<IDataInitMetadata> GetData()
        {
            var data = new List<IDataInitMetadata>();

            if (_element != null)
            {
                var elements = _element.Value.Value<JArray>();

                if (elements != null)
                {
                    foreach (var node in elements)
                    {
                        data.Add(new DataInitMetadata((JObject) node));
                    }
                }
            }

            return data;
        }
    }
}
