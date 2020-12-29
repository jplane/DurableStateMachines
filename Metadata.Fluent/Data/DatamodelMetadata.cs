using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System.Collections.Generic;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Data
{
    public sealed class DatamodelMetadata<TParent> : IDatamodelMetadata where TParent : IModelMetadata
    {
        private readonly List<DataInitMetadata<DatamodelMetadata<TParent>>> _dataInits;

        internal DatamodelMetadata()
        {
            _dataInits = new List<DataInitMetadata<DatamodelMetadata<TParent>>>();
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.Write(this.MetadataId);

            writer.WriteMany(_dataInits, (o, w) => o.Serialize(w));
        }

        internal static DatamodelMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new DatamodelMetadata<TParent>();

            metadata.MetadataId = reader.ReadString();

            metadata._dataInits.AddRange(reader.ReadMany(DataInitMetadata<DatamodelMetadata<TParent>>.Deserialize,
                                           o => o.Parent = metadata));

            return metadata;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        public DataInitMetadata<DatamodelMetadata<TParent>> DataInit()
        {
            var datainit = new DataInitMetadata<DatamodelMetadata<TParent>>();

            datainit.Parent = this;

            _dataInits.Add(datainit);

            datainit.MetadataId = $"{((IModelMetadata)this).MetadataId}.DataInits[{_dataInits.Count}]";

            return datainit;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        string IModelMetadata.MetadataId => this.MetadataId;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        IEnumerable<IDataInitMetadata> IDatamodelMetadata.GetData() => _dataInits;
    }
}
