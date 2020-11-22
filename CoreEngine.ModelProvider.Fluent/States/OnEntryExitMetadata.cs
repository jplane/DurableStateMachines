using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States
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

        internal TParent Parent { get; set; }

        internal string UniqueId { private get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public AssignMetadata<OnEntryExitMetadata<TParent>> WithAssign()
        {
            var ec = new AssignMetadata<OnEntryExitMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<OnEntryExitMetadata<TParent>> WithCancel()
        {
            var ec = new CancelMetadata<OnEntryExitMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<OnEntryExitMetadata<TParent>> WithForeach()
        {
            var ec = new ForeachMetadata<OnEntryExitMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<OnEntryExitMetadata<TParent>> WithIf()
        {
            var ec = new IfMetadata<OnEntryExitMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public OnEntryExitMetadata<TParent> WithLog(string message)
        {
            var ec = new LogMetadata<OnEntryExitMetadata<TParent>>();

            ec.WithMessage(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public OnEntryExitMetadata<TParent> WithLog(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<OnEntryExitMetadata<TParent>>();

            ec.WithMessage(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public OnEntryExitMetadata<TParent> WithRaise(string messageName)
        {
            var ec = new RaiseMetadata<OnEntryExitMetadata<TParent>>();

            ec.WithMessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public OnEntryExitMetadata<TParent> WithScript(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<OnEntryExitMetadata<TParent>>();

            ec.WithAction(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<OnEntryExitMetadata<TParent>> WithSendMessage()
        {
            var ec = new SendMessageMetadata<OnEntryExitMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        bool IOnEntryExitMetadata.IsEntry => _isEntry;

        string IModelMetadata.UniqueId => this.UniqueId;

        IEnumerable<IExecutableContentMetadata> IOnEntryExitMetadata.GetExecutableContent() => _executableContent;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }
    }
}
