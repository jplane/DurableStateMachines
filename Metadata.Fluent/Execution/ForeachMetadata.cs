using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class ForeachMetadata<TParent> : ExecutableContentMetadata, IForeachMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;
        private string _itemLocation;
        private string _indexLocation;
        private IEnumerable _items;
        private Func<dynamic, IEnumerable> _getItems;

        internal ForeachMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_itemLocation);
            writer.WriteNullableString(_indexLocation);
            writer.WriteObject(_items);
            writer.Write(_getItems);

            writer.WriteMany(_executableContent, (o, w) => o.Serialize(w));
        }

        internal static ForeachMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new ForeachMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            metadata._itemLocation = reader.ReadNullableString();
            metadata._indexLocation = reader.ReadNullableString();
            metadata._items = (IEnumerable) reader.ReadObject();
            metadata._getItems = reader.Read<Func<dynamic, IEnumerable>>();
            metadata._executableContent.AddRange(ExecutableContentMetadata.DeserializeMany(reader, metadata));

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public ForeachMetadata<TParent> ItemLocation(string itemLocation)
        {
            _itemLocation = itemLocation;
            return this;
        }

        public ForeachMetadata<TParent> IndexLocation(string indexLocation)
        {
            _indexLocation = indexLocation;
            return this;
        }

        public ForeachMetadata<TParent> Items(IEnumerable items)
        {
            _items = items;
            _getItems = null;
            return this;
        }

        public ForeachMetadata<TParent> Items(Func<dynamic, IEnumerable> getter)
        {
            _getItems = getter;
            _items = null;
            return this;
        }

        public AssignMetadata<ForeachMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<ForeachMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<ForeachMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<ForeachMetadata<TParent>> If()
        {
            var ec = new IfMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<ForeachMetadata<TParent>>();

            ec.Message(message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ForeachMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<ForeachMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ForeachMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<ForeachMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ForeachMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<ForeachMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<ForeachMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        internal QueryMetadata<ForeachMetadata<TParent>> Query()
        {
            var ec = new QueryMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        string IForeachMetadata.Item => _itemLocation;

        string IForeachMetadata.Index => _indexLocation;

        IEnumerable IForeachMetadata.GetArray(dynamic data) =>
            (_getItems == null ? _items : _getItems.Invoke(data)) ?? Enumerable.Empty<object>();

        IEnumerable<IExecutableContentMetadata> IForeachMetadata.GetExecutableContent() => _executableContent;
    }
}
