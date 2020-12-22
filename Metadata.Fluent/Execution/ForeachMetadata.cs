using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class ForeachMetadata<TParent> : ExecutableContentMetadata, IForeachMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;
        private string _itemLocation;
        private string _indexLocation;
        private Func<dynamic, IEnumerable> _getItems;

        internal ForeachMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
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

        public ForeachMetadata<TParent> Items(Func<dynamic, IEnumerable> getter)
        {
            _getItems = getter;
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

            ec.Message(_ => message);

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

        IEnumerable IForeachMetadata.GetArray(dynamic data) => _getItems?.Invoke(data) ?? Enumerable.Empty<object>();

        IEnumerable<IExecutableContentMetadata> IForeachMetadata.GetExecutableContent() => _executableContent;
    }
}
