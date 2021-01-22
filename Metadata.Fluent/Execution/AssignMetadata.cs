using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class AssignMetadata<TParent> : ExecutableContentMetadata, IAssignMetadata where TParent : IModelMetadata
    {
        private readonly Lazy<Func<IDictionary<string, object>, object>> _valueResolver;

        private string _location;
        private object _value;
        private Expression<Func<IDictionary<string, object>, object>> _getValue;

        internal AssignMetadata()
        {
            _valueResolver = new Lazy<Func<IDictionary<string, object>, object>>(() =>
                _getValue == null ? _ => _value : _getValue.Compile());
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_location);
            writer.WriteObject(_value);
            writer.Write(_getValue);
        }

        internal static AssignMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new AssignMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            metadata._location = reader.ReadNullableString();
            metadata._value = reader.ReadObject();
            metadata._getValue = reader.Read<Expression<Func<IDictionary<string, object>, object>>>();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public AssignMetadata<TParent> Location(string location)
        {
            _location = location;
            return this;
        }

        public AssignMetadata<TParent> Value(object value)
        {
            _value = value;
            _getValue = null;
            return this;
        }

        public AssignMetadata<TParent> Value(Expression<Func<IDictionary<string, object>, object>> getter)
        {
            _getValue = getter;
            _value = null;
            return this;
        }

        public TParent _ => this.Parent;

        string IAssignMetadata.Location => _location;

        object IAssignMetadata.GetValue(dynamic data) => _valueResolver.Value(data);
    }
}
