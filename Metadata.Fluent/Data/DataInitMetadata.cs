using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.Data
{
    public sealed class DataInitMetadata<TParent> : IDataInitMetadata where TParent : IDatamodelMetadata
    {
        private string _id;
        private Func<dynamic, object> _valueGetter;

        internal DataInitMetadata()
        {
        }

        public DataInitMetadata<TParent> Id(string id)
        {
            _id = id;
            return this;
        }

        public DataInitMetadata<TParent> Value(Func<dynamic, object> getter)
        {
            _valueGetter = getter;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        string IModelMetadata.MetadataId => this.MetadataId;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        string IDataInitMetadata.Id => _id;

        object IDataInitMetadata.GetValue(dynamic data) => _valueGetter?.Invoke(data);
    }
}
