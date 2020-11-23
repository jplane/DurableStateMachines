using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.DataManipulation
{
    public sealed class DatamodelMetadata<TParent> : IDatamodelMetadata where TParent : IModelMetadata
    {
        private readonly List<DataInitMetadata<DatamodelMetadata<TParent>>> _dataInits;

        internal DatamodelMetadata()
        {
            _dataInits = new List<DataInitMetadata<DatamodelMetadata<TParent>>>();
        }

        internal TParent Parent { get; set; }

        internal string UniqueId { private get; set; }

        public DatamodelMetadata<TParent> DataInit()
        {
            var datainit = new DataInitMetadata<DatamodelMetadata<TParent>>();

            datainit.Parent = this;

            _dataInits.Add(datainit);

            datainit.UniqueId = $"{((IModelMetadata)this).UniqueId}.DataInits[{_dataInits.Count}]";

            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        string IModelMetadata.UniqueId => this.UniqueId;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        IEnumerable<IDataInitMetadata> IDatamodelMetadata.GetData() => _dataInits;
    }
}
