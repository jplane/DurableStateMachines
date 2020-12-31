using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class RaiseMetadata<TParent> : ExecutableContentMetadata, IRaiseMetadata where TParent : IModelMetadata
    {
        private string _messageName;

        internal RaiseMetadata()
        {
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_messageName);
        }

        internal static RaiseMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new RaiseMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();

            metadata._messageName = reader.ReadNullableString();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public RaiseMetadata<TParent> MessageName(string messageName)
        {
            _messageName = messageName;
            return this;
        }

        string IRaiseMetadata.MessageName => _messageName;
    }
}
