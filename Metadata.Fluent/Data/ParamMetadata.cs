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
        private object _value;
        private Func<dynamic, object> _valueGetter;

        internal ParamMetadata(string name)
        {
            _name = name;
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.Write(_name);
            writer.Write(this.MetadataId);
            writer.WriteObject(_value);
            writer.Write(_valueGetter);
        }

        internal static ParamMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var name = reader.ReadString();

            var metadata = new ParamMetadata<TParent>(name);

            metadata.MetadataId = reader.ReadString();
            metadata._value = reader.ReadObject();
            metadata._valueGetter = reader.Read<Func<dynamic, object>>();

            return metadata;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        public ParamMetadata<TParent> Value(object value)
        {
            _value = value;
            _valueGetter = null;
            return this;
        }

        public ParamMetadata<TParent> Value(Func<dynamic, object> getter)
        {
            _valueGetter = getter;
            _value = null;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        internal bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        internal string Name => _name;

        internal object GetValue(dynamic data) =>
            _valueGetter == null ? _value : _valueGetter.Invoke(data);
    }
}
