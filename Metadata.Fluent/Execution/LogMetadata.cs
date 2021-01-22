using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class LogMetadata<TParent> : ExecutableContentMetadata, ILogMetadata where TParent : IModelMetadata
    {
        private readonly Lazy<Func<IDictionary<string, object>, string>> _messageResolver;

        private string _message;
        private Expression<Func<IDictionary<string, object>, string>> _getMessage;

        internal LogMetadata()
        {
            _messageResolver = new Lazy<Func<IDictionary<string, object>, string>>(() =>
                _getMessage == null ? _ => _message : _getMessage.Compile());
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_message);
            writer.Write(_getMessage);
        }

        internal static LogMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new LogMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();

            metadata._message = reader.ReadNullableString();
            metadata._getMessage = reader.Read<Expression<Func<IDictionary<string, object>, string>>>();

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent _ => this.Parent;

        public LogMetadata<TParent> Message(string message)
        {
            _message = message;
            _getMessage = null;
            return this;
        }

        public LogMetadata<TParent> Message(Expression<Func<IDictionary<string, object>, string>> getter)
        {
            _getMessage = getter;
            _message = null;
            return this;
        }

        string ILogMetadata.GetMessage(dynamic data) => _messageResolver.Value(data) ?? string.Empty;
    }
}
