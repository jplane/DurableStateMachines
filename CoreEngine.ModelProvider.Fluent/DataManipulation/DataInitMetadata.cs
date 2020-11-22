using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.DataManipulation
{
    public sealed class DataInitMetadata<TParent> : IDataInitMetadata where TParent : IDatamodelMetadata
    {
        private string _id;
        private Func<dynamic, object> _valueGetter;

        internal DataInitMetadata()
        {
        }

        public DataInitMetadata<TParent> WithId(string id)
        {
            _id = id;
            return this;
        }

        public DataInitMetadata<TParent> WithValue(Func<dynamic, object> getter)
        {
            _valueGetter = getter;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        internal TParent Parent { get; set; }

        internal string UniqueId { private get; set; }

        string IModelMetadata.UniqueId => this.UniqueId;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        string IDataInitMetadata.Id => _id;

        object IDataInitMetadata.GetValue(dynamic data) => _valueGetter?.Invoke(data);
    }
}
