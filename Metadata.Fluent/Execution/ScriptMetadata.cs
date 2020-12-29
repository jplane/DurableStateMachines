using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class ScriptMetadata<TParent> : ExecutableContentMetadata, IScriptMetadata where TParent : IModelMetadata
    {
        private Action<dynamic> _action;

        internal ScriptMetadata()
        {
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

            metadata.MetadataId = reader.ReadString();

            metadata._action = reader.Read<Action<dynamic>>();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public ScriptMetadata<TParent> Action(Action<dynamic> action)
        {
            _action = action;
            return this;
        }

        void IScriptMetadata.Execute(dynamic data) => _action?.Invoke(data);
    }
}
