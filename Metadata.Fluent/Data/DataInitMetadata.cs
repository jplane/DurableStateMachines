using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Data
{
    public sealed class DataInitMetadata<TParent> : IDataInitMetadata where TParent : IDatamodelMetadata
    {
        private string _id;
        private object _value;
        private Func<dynamic, object> _valueGetter;

        internal DataInitMetadata()
        {
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.Write(_id);
            writer.Write(this.MetadataId);
            writer.WriteObject(_value);
            writer.Write(_valueGetter);
        }

        internal static DataInitMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new DataInitMetadata<TParent>();

            metadata._id = reader.ReadString();
            metadata.MetadataId = reader.ReadString();
            metadata._value = reader.ReadObject();
            metadata._valueGetter = reader.Read<Func<dynamic, object>>();

            return metadata;
        }

        public DataInitMetadata<TParent> Id(string id)
        {
            _id = id;
            return this;
        }

        public DataInitMetadata<TParent> Value(object value)
        {
            _value = value;
            _valueGetter = null;
            return this;
        }

        public DataInitMetadata<TParent> Value(Func<dynamic, object> getter)
        {
            _valueGetter = getter;
            _value = null;
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

        object IDataInitMetadata.GetValue(dynamic data) =>
            _valueGetter == null ? _value : _valueGetter.Invoke(data);
    }
}
