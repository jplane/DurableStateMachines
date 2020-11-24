using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Execution;
using System;
using System.Collections.Generic;

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

        internal TParent Parent { get; set; }

        internal string UniqueId { private get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

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

        public AssignMetadata<TransitionMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<TransitionMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<TransitionMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<TransitionMetadata<TParent>> If()
        {
            var ec = new IfMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public TransitionMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<TransitionMetadata<TParent>>();

            ec.Message(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<TransitionMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<TransitionMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<TransitionMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<TransitionMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        IEnumerable<string> ITransitionMetadata.Targets => _targets;

        IEnumerable<string> ITransitionMetadata.Messages => _messages;

        TransitionType ITransitionMetadata.Type => _type;

        string IModelMetadata.UniqueId => this.UniqueId;

        bool ITransitionMetadata.EvalCondition(dynamic data) => _evalCondition?.Invoke(data) ?? true;

        IEnumerable<IExecutableContentMetadata> ITransitionMetadata.GetExecutableContent() => _executableContent;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
