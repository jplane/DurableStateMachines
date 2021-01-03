using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class TransitionMetadata<TParent> : ITransitionMetadata where TParent : IModelMetadata
    {
        private readonly List<string> _targets;
        private readonly List<string> _messages;
        private readonly List<ExecutableContentMetadata> _executableContent;

        private TransitionType _type;
        private Func<dynamic, bool> _evalCondition;

        internal TransitionMetadata()
        {
            _type = TransitionType.External;

            _targets = new List<string>();
            _messages = new List<string>();
            _executableContent = new List<ExecutableContentMetadata>();
        }

        internal void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.WriteNullableString(this.MetadataId);
            writer.Write((int) _type);
            writer.Write(_evalCondition);

            writer.WriteNullableString(string.Join('|', _targets));
            writer.WriteNullableString(string.Join('|', _messages));

            writer.WriteMany(_executableContent, (o, w) => o.Serialize(w));
        }

        internal static TransitionMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new TransitionMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            metadata._type = (TransitionType)reader.ReadInt32();
            metadata._evalCondition = reader.Read<Func<dynamic, bool>>();

            metadata._targets.AddRange(reader.ReadNullableString().Split('|'));
            metadata._messages.AddRange(reader.ReadNullableString().Split('|'));

            metadata._executableContent.AddRange(ExecutableContentMetadata.DeserializeMany(reader, metadata));

            return metadata;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        public TParent _ => this.Parent;

        public TransitionMetadata<TParent> Type(TransitionType type)
        {
            _type = type;
            return this;
        }

        public TransitionMetadata<TParent> Target(string target)
        {
            _targets.Add(target);
            return this;
        }

        public TransitionMetadata<TParent> Message(string message)
        {
            _messages.Add(message);
            return this;
        }

        public TransitionMetadata<TParent> Condition(Func<dynamic, bool> condition)
        {
            _evalCondition = condition;
            return this;
        }

        public TransitionMetadata<TParent> Assign(string location, object value)
        {
            var ec = new AssignMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(value);

            return this;
        }

        public TransitionMetadata<TParent> Assign(string location, Func<dynamic, object> getter)
        {
            var ec = new AssignMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            ec.Location(location).Value(getter);

            return this;
        }

        public CancelMetadata<TransitionMetadata<TParent>> Cancel
        {
            get
            {
                var ec = new CancelMetadata<TransitionMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public ForeachMetadata<TransitionMetadata<TParent>> Foreach
        {
            get
            {
                var ec = new ForeachMetadata<TransitionMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public IfMetadata<TransitionMetadata<TParent>> If
        {
            get
            {
                var ec = new IfMetadata<TransitionMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        public TransitionMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<TransitionMetadata<TParent>>();

            ec.Message(message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<TransitionMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<TransitionMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<TransitionMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<TransitionMetadata<TParent>> SendMessage
        {
            get
            {
                var ec = new SendMessageMetadata<TransitionMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        internal QueryMetadata<TransitionMetadata<TParent>> Query
        {
            get
            {
                var ec = new QueryMetadata<TransitionMetadata<TParent>>();

                _executableContent.Add(ec);

                ec.Parent = this;

                ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

                return ec;
            }
        }

        IEnumerable<string> ITransitionMetadata.Targets => _targets;

        IEnumerable<string> ITransitionMetadata.Messages => _messages;

        TransitionType ITransitionMetadata.Type => _type;

        string IModelMetadata.MetadataId => this.MetadataId;

        bool ITransitionMetadata.EvalCondition(dynamic data) => _evalCondition?.Invoke(data) ?? true;

        IEnumerable<IExecutableContentMetadata> ITransitionMetadata.GetExecutableContent() => _executableContent;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
