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
    public sealed class ScriptMetadata<TParent> : ExecutableContentMetadata, IScriptMetadata where TParent : IModelMetadata
    {
        private readonly Lazy<Action<IDictionary<string, object>>> _actionResolver;

        private Expression<Action<IDictionary<string, object>>> _action;

        internal ScriptMetadata()
        {
            _actionResolver = new Lazy<Action<IDictionary<string, object>>>(() =>
                _action == null ? _ => { } : _action.Compile());
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write(_action);
        }

        internal static ScriptMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new ScriptMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();

            metadata._action = reader.Read<Expression<Action<IDictionary<string, object>>>>();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent _ => this.Parent;

        public ScriptMetadata<TParent> Action(Expression<Action<IDictionary<string, object>>> action)
        {
            _action = action;
            return this;
        }

        void IScriptMetadata.Execute(dynamic data) => _actionResolver.Value(data);
    }
}
