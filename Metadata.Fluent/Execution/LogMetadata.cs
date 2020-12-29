using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class LogMetadata<TParent> : ExecutableContentMetadata, ILogMetadata where TParent : IModelMetadata
    {
        private string _message;
        private Func<dynamic, string> _getMessage;

        internal LogMetadata()
        {
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write(_message);
            writer.Write(_getMessage);
        }

        internal static LogMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new LogMetadata<TParent>();

            metadata.MetadataId = reader.ReadString();

            metadata._message = reader.ReadString();
            metadata._getMessage = reader.Read<Func<dynamic, string>>();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public LogMetadata<TParent> Message(string message)
        {
            _message = message;
            _getMessage = null;
            return this;
        }

        public LogMetadata<TParent> Message(Func<dynamic, string> getter)
        {
            _getMessage = getter;
            _message = null;
            return this;
        }

        string ILogMetadata.GetMessage(dynamic data) =>
            (_getMessage == null ? _message : _getMessage.Invoke(data)) ?? string.Empty;
    }
}
