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

        public ElseIfMetadata<TParent> Condition(Func<dynamic, bool> condition)
        {
            _eval = condition;
            return this;
        }

        public AssignMetadata<ElseIfMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<ElseIfMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<ElseIfMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<ElseIfMetadata<TParent>> If()
        {
            var ec = new IfMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ElseIfMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<ElseIfMetadata<TParent>>();

            ec.Message(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseIfMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<ElseIfMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseIfMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<ElseIfMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseIfMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<ElseIfMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<ElseIfMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<ElseIfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }
    }
}
