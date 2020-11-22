using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
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

        public ForeachMetadata<TParent> WithItemLocation(string itemLocation)
        {
            _itemLocation = itemLocation;
            return this;
        }

        public ForeachMetadata<TParent> WithIndexLocation(string indexLocation)
        {
            _indexLocation = indexLocation;
            return this;
        }

        public ForeachMetadata<TParent> WithItems(Func<dynamic, IEnumerable> getter)
        {
            _getItems = getter;
            return this;
        }

        public AssignMetadata<ForeachMetadata<TParent>> WithAssign()
        {
            var ec = new AssignMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<ForeachMetadata<TParent>> WithCancel()
        {
            var ec = new CancelMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<ForeachMetadata<TParent>> WithForeach()
        {
            var ec = new ForeachMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<ForeachMetadata<TParent>> WithIf()
        {
            var ec = new IfMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<TParent> WithLog(string message)
        {
            var ec = new LogMetadata<ForeachMetadata<TParent>>();

            ec.WithMessage(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ForeachMetadata<TParent> WithLog(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<ForeachMetadata<TParent>>();

            ec.WithMessage(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ForeachMetadata<TParent> WithRaise(string messageName)
        {
            var ec = new RaiseMetadata<ForeachMetadata<TParent>>();

            ec.WithMessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ForeachMetadata<TParent> WithScript(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<ForeachMetadata<TParent>>();

            ec.WithAction(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<ForeachMetadata<TParent>> WithSendMessage()
        {
            var ec = new SendMessageMetadata<ForeachMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        string IForeachMetadata.Item => _itemLocation;

        string IForeachMetadata.Index => _indexLocation;

        IEnumerable IForeachMetadata.GetArray(dynamic data) => _getItems?.Invoke(data) ?? Enumerable.Empty<object>();

        IEnumerable<IExecutableContentMetadata> IForeachMetadata.GetExecutableContent() => _executableContent;
    }
}
