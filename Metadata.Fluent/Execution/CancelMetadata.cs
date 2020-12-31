using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class CancelMetadata<TParent> : ExecutableContentMetadata, ICancelMetadata where TParent : IModelMetadata
    {
        private string _sendId;
        private string _sendIdExpr;

        internal CancelMetadata()
        {
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_sendId);
            writer.WriteNullableString(_sendIdExpr);
        }

        internal static CancelMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new CancelMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            metadata._sendId = reader.ReadNullableString();
            metadata._sendIdExpr = reader.ReadNullableString();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public CancelMetadata<TParent> SendId(string sendId)
        {
            _sendId = sendId;
            return this;
        }

        public CancelMetadata<TParent> SendIdExpression(string sendIdExpr)
        {
            _sendIdExpr = sendIdExpr;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        string ICancelMetadata.SendId => _sendId;

        string ICancelMetadata.SendIdExpr => _sendIdExpr;
    }
}
