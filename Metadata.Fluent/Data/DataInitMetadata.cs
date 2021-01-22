using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Fluent.Data
{
    public sealed class DataInitMetadata<TParent> : IDataInitMetadata where TParent : IDatamodelMetadata
    {
        private readonly Lazy<Func<IDictionary<string, object>, object>> _valueResolver;

        private string _id;
        private object _value;
        private Expression<Func<IDictionary<string, object>, object>> _valueGetter;

        internal DataInitMetadata()
        {
            _valueResolver = new Lazy<Func<IDictionary<string, object>, object>>(() =>
                _valueGetter == null ? _ => _value : _valueGetter.Compile());
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.WriteNullableString(_id);
            writer.WriteNullableString(this.MetadataId);
            writer.WriteObject(_value);
            writer.Write(_valueGetter);
        }

        internal static DataInitMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new DataInitMetadata<TParent>();

            metadata._id = reader.ReadNullableString();
            metadata.MetadataId = reader.ReadNullableString();
            metadata._value = reader.ReadObject();
            metadata._valueGetter = reader.Read<Expression<Func<IDictionary<string, object>, object>>>();

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

        public DataInitMetadata<TParent> Value(Expression<Func<IDictionary<string, object>, object>> getter)
        {
            _valueGetter = getter;
            _value = null;
            return this;
        }

        public IReadOnlyDictionary<string, object> DebuggerInfo => null;

        public TParent _ => this.Parent;

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        string IModelMetadata.MetadataId => this.MetadataId;

        string IDataInitMetadata.Id => _id;

        object IDataInitMetadata.GetValue(dynamic data) => _valueResolver.Value(data);
    }
}
