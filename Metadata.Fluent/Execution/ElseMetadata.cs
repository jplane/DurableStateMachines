using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class ElseMetadata<TParent> : ExecutableContentMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;

        internal ElseMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteMany(_executableContent, (o, w) => o.Serialize(w));
        }

        internal static ElseMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new ElseMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();

            metadata._executableContent.AddRange(ExecutableContentMetadata.DeserializeMany(reader, metadata));

            return metadata;
        }

        internal TParent Parent { get; set; }

        internal IEnumerable<ExecutableContentMetadata> GetExecutableContent() => _executableContent;

        public TParent _ => this.Parent;

        public ElseMetadata<TParent> Assign(string location, object value)
        {
            var ec = new AssignMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(value);

            return this;
        }

        public ElseMetadata<TParent> Assign(string location, Func<dynamic, object> getter)
        {
            var ec = new AssignMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(getter);

            return this;
        }

        public CancelMetadata<ElseMetadata<TParent>> Cancel
        {
            get
            {
                var ec = new CancelMetadata<ElseMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public ForeachMetadata<ElseMetadata<TParent>> Foreach
        {
            get
            {
                var ec = new ForeachMetadata<ElseMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public IfMetadata<ElseMetadata<TParent>> If
        {
            get
            {
                var ec = new IfMetadata<ElseMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public ElseMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<ElseMetadata<TParent>>();

            ec.Message(message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<ElseMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<ElseMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<ElseMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<ElseMetadata<TParent>> SendMessage
        {
            get
            {
                var ec = new SendMessageMetadata<ElseMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        internal QueryMetadata<ElseMetadata<TParent>> Query
        {
            get
            {
                var ec = new QueryMetadata<ElseMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }
    }
}
