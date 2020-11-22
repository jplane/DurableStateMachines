using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class ElseIfMetadata<TParent> : ExecutableContentMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;
        private Func<dynamic, bool> _eval;

        internal ElseIfMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
        }

        internal TParent Parent { get; set; }

        internal IEnumerable<ExecutableContentMetadata> GetExecutableContent() => _executableContent;

        internal Func<dynamic, bool> GetEvalCondition() => _eval;

        public TParent Attach()
        {
            return this.Parent;
        }

        public ElseIfMetadata<TParent> WithCondition(Func<dynamic, bool> condition)
        {
            _eval = condition;
            return this;
        }

        public AssignMetadata<ElseIfMetadata<TParent>> WithAssign()
        {
            var ec = new AssignMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<ElseIfMetadata<TParent>> WithCancel()
        {
            var ec = new CancelMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<ElseIfMetadata<TParent>> WithForeach()
        {
            var ec = new ForeachMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<ElseIfMetadata<TParent>> WithIf()
        {
            var ec = new IfMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ElseIfMetadata<TParent> WithLog(string message)
        {
            var ec = new LogMetadata<ElseIfMetadata<TParent>>();

            ec.WithMessage(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseIfMetadata<TParent> WithLog(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<ElseIfMetadata<TParent>>();

            ec.WithMessage(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseIfMetadata<TParent> WithRaise(string messageName)
        {
            var ec = new RaiseMetadata<ElseIfMetadata<TParent>>();

            ec.WithMessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseIfMetadata<TParent> WithScript(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<ElseIfMetadata<TParent>>();

            ec.WithAction(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<ElseIfMetadata<TParent>> WithSendMessage()
        {
            var ec = new SendMessageMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }
    }
}
