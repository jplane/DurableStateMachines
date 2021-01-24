using Newtonsoft.Json;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateChartsDotNet.Metadata.Data
{
    public class DataModel : IDataModelMetadata
    {
        private MetadataList<DataInit> _items;

        public DataModel()
        {
            this.Items = new MetadataList<DataInit>();
        }

        [JsonProperty("items")]
        public MetadataList<DataInit> Items
        {
            get => _items;
            
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_items != null)
                {
                    _items.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "datamodel"}.items";

                _items = value;
            }
        }

        internal void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            foreach (var init in this.Items)
            {
                init.Validate(errorMap);
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["metadataId"] = ((IModelMetadata) this).MetadataId;

                return info;
            }
        }

        string IModelMetadata.MetadataId => this.MetadataIdResolver?.Invoke(this);

        internal Func<IModelMetadata, string> MetadataIdResolver { private get; set; }

        IEnumerable<IDataInitMetadata> IDataModelMetadata.GetData() => this.Items ?? Enumerable.Empty<IDataInitMetadata>();
    }
}
