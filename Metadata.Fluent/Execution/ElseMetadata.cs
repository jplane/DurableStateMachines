using StateChartsDotNet.Common.Model;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.Execution
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

        public AssignMetadata<ElseMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<ElseMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<ElseMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<ElseMetadata<TParent>> If()
        {
            var ec = new IfMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ElseMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<ElseMetadata<TParent>>();

            ec.Message(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<ElseMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<ElseMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public ElseMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<ElseMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<ElseMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<ElseMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }
    }
}
