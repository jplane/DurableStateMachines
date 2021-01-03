using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Data
{
    public sealed class ParamMetadata<TParent> where TParent : IModelMetadata
    {
        private readonly string _name;

        private string _location;
        private object _value;
        private Func<dynamic, object> _valueGetter;

        internal ParamMetadata(string name)
        {
            _name = name;
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
            metadata._valueGetter = reader.Read<Func<dynamic, object>>();

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

        public ParamMetadata<TParent> Value(Func<dynamic, object> getter)
        {
            _valueGetter = getter;
            _value = null;
            _location = null;
            return this;
        }

        public TParent _ => this.Parent;

        internal bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        internal string Name => _name;

        internal object GetValue(dynamic data)
        {
            if (!string.IsNullOrWhiteSpace(_location))
            {
                return data[_location];
            }
            else if (_valueGetter == null)
            {
                return _value;
            }
            else
            {
                return _valueGetter.Invoke(data);
            }
        }
    }
}
