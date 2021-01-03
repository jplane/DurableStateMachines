using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class OnEntryExitMetadata<TParent> : IOnEntryExitMetadata where TParent : IModelMetadata
    {
        private readonly bool _isEntry;
        private readonly List<ExecutableContentMetadata> _executableContent;

        internal OnEntryExitMetadata(bool isEntry)
        {
            _isEntry = isEntry;
            _executableContent = new List<ExecutableContentMetadata>();
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.Write(_isEntry);
            writer.WriteNullableString(this.MetadataId);

            writer.WriteMany(_executableContent, (o, w) => o.Serialize(w));
        }

        internal static OnEntryExitMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var isEntry = reader.ReadBoolean();

            var metadata = new OnEntryExitMetadata<TParent>(isEntry);

            metadata.MetadataId = reader.ReadNullableString();

            metadata._executableContent.AddRange(ExecutableContentMetadata.DeserializeMany(reader, metadata));

            return metadata;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        public TParent _ => this.Parent;

        public OnEntryExitMetadata<TParent> Assign(string location, object value)
        {
            var ec = new AssignMetadata<OnEntryExitMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(value);

            return this;
        }

        public OnEntryExitMetadata<TParent> Assign(string location, Func<dynamic, object> getter)
        {
            var ec = new AssignMetadata<OnEntryExitMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(getter);

            return this;
        }

        public CancelMetadata<OnEntryExitMetadata<TParent>> Cancel
        {
            get
            {
                var ec = new CancelMetadata<OnEntryExitMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public ForeachMetadata<OnEntryExitMetadata<TParent>> Foreach
        {
            get
            {
                var ec = new ForeachMetadata<OnEntryExitMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public IfMetadata<OnEntryExitMetadata<TParent>> If
        {
            get
            {
                var ec = new IfMetadata<OnEntryExitMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public OnEntryExitMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<OnEntryExitMetadata<TParent>>();

            ec.Message(message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public OnEntryExitMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<OnEntryExitMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public OnEntryExitMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<OnEntryExitMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public OnEntryExitMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<OnEntryExitMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<OnEntryExitMetadata<TParent>> SendMessage
        {
            get
            {
                var ec = new SendMessageMetadata<OnEntryExitMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        internal QueryMetadata<OnEntryExitMetadata<TParent>> Query
        {
            get
            {
                var ec = new QueryMetadata<OnEntryExitMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        bool IOnEntryExitMetadata.IsEntry => _isEntry;

        string IModelMetadata.MetadataId => this.MetadataId;

        IEnumerable<IExecutableContentMetadata> IOnEntryExitMetadata.GetExecutableContent() => _executableContent;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }
    }
}
