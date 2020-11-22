using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class ElseMetadata<TParent> : ExecutableContentMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;

        internal ElseMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
        }

        internal TParent Parent { get; set; }

        internal IEnumerable<ExecutableContentMetadata> GetExecutableContent() => _executableContent;

        public TParent Attach()
        {
            return this.Parent;
        }

        public AssignMetadata<ElseMetadata<TParent>> WithAssign()
        {
            var ec = new AssignMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<ElseMetadata<TParent>> WithCancel()
        {
            var ec = new CancelMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<ElseMetadata<TParent>> WithForeach()
        {
            var ec = new ForeachMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<ElseMetadata<TParent>> WithIf()
        {
            var ec = new IfMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ElseMetadata<TParent> WithLog(string message)
        {
            var ec = new LogMetadata<ElseMetadata<TParent>>();

            ec.WithMessage(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> WithLog(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<ElseMetadata<TParent>>();

            ec.WithMessage(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> WithRaise(string messageName)
        {
            var ec = new RaiseMetadata<ElseMetadata<TParent>>();

            ec.WithMessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> WithScript(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<ElseMetadata<TParent>>();

            ec.WithAction(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<ElseMetadata<TParent>> WithSendMessage()
        {
            var ec = new SendMessageMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }
    }
}
