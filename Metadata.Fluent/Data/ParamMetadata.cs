using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Fluent.Data
{
    public sealed class ParamMetadata<TParent> where TParent : IModelMetadata
    {
        private readonly string _name;
        private readonly Lazy<Func<IDictionary<string, object>, object>> _valueResolver;

        private string _location;
        private object _value;
        private Expression<Func<IDictionary<string, object>, object>> _valueGetter;

        internal ParamMetadata(string name)
        {
            _name = name;
            _valueResolver = new Lazy<Func<IDictionary<string, object>, object>>(() =>
                _valueGetter == null ? _ => _value : _valueGetter.Compile());
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.WriteNullableString(_name);
            writer.WriteNullableString(this.MetadataId);
            writer.WriteNullableString(_location);
            writer.WriteObject(_value);
            writer.Write(_valueGetter);
        }

        internal static ParamMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var name = reader.ReadNullableString();

            var metadata = new ParamMetadata<TParent>(name);

            metadata.MetadataId = reader.ReadNullableString();
            metadata._location = reader.ReadNullableString();
            metadata._value = reader.ReadObject();
            metadata._valueGetter = reader.Read<Expression<Func<IDictionary<string, object>, object>>>();

            return metadata;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        public ParamMetadata<TParent> Location(string location)
        {
            _location = location;
            _value = null;
            _valueGetter = null;
            return this;
        }

        public ParamMetadata<TParent> Value(object value)
        {
            _value = value;
            _valueGetter = null;
            _location = null;
            return this;
        }

        public ParamMetadata<TParent> Value(Expression<Func<IDictionary<string, object>, object>> getter)
        {
            _valueGetter = getter;
            _value = null;
            _location = null;
            return this;
        }

        public TParent _ => this.Parent;

        internal bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }

        internal string Name => _name;

        internal object GetValue(dynamic data)
        {
            if (!string.IsNullOrWhiteSpace(_location))
            {
                return data[_location];
            }
            else
            {
                return _valueResolver.Value(data);
            }
        }
    }
}
