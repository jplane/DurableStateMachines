using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States
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

        public TransitionMetadata<TParent> WithType(TransitionType type)
        {
            _type = type;
            return this;
        }

        public TransitionMetadata<TParent> WithTarget(string target)
        {
            _targets.Add(target);
            return this;
        }

        public TransitionMetadata<TParent> WithMessage(string message)
        {
            _messages.Add(message);
            return this;
        }

        public TransitionMetadata<TParent> WithCondition(Func<dynamic, bool> condition)
        {
            _evalCondition = condition;
            return this;
        }

        public AssignMetadata<TransitionMetadata<TParent>> WithAssign()
        {
            var ec = new AssignMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<TransitionMetadata<TParent>> WithCancel()
        {
            var ec = new CancelMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<TransitionMetadata<TParent>> WithForeach()
        {
            var ec = new ForeachMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<TransitionMetadata<TParent>> WithIf()
        {
            var ec = new IfMetadata<TransitionMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public TransitionMetadata<TParent> WithLog(string message)
        {
            var ec = new LogMetadata<TransitionMetadata<TParent>>();

            ec.WithMessage(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> WithLog(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<TransitionMetadata<TParent>>();

            ec.WithMessage(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> WithRaise(string messageName)
        {
            var ec = new RaiseMetadata<TransitionMetadata<TParent>>();

            ec.WithMessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public TransitionMetadata<TParent> WithScript(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<TransitionMetadata<TParent>>();

            ec.WithAction(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<TransitionMetadata<TParent>> WithSendMessage()
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
