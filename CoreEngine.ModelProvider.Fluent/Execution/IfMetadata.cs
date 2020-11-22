using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class IfMetadata<TParent> : ExecutableContentMetadata, IIfMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;
        private readonly List<ElseIfMetadata<IfMetadata<TParent>>> _elseIfs;
        private ElseMetadata<IfMetadata<TParent>> _else;

        private Func<dynamic, bool> _eval;

        internal IfMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
            _elseIfs = new List<ElseIfMetadata<IfMetadata<TParent>>>();
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public IfMetadata<TParent> WithCondition(Func<dynamic, bool> condition)
        {
            _eval = condition;
            return this;
        }

        public ElseIfMetadata<IfMetadata<TParent>> WithElseIf()
        {
            var elseif = new ElseIfMetadata<IfMetadata<TParent>>();

            _elseIfs.Add(elseif);

            elseif.Parent = this;

            elseif.UniqueId = $"{((IModelMetadata)this).UniqueId}.ElseIf[{_elseIfs.Count}]";

            return elseif;
        }

        public ElseMetadata<IfMetadata<TParent>> WithElse()
        {
            _else = new ElseMetadata<IfMetadata<TParent>>();

            _else.Parent = this;

            _else.UniqueId = $"{((IModelMetadata)this).UniqueId}.Else";

            return _else;
        }

        public AssignMetadata<IfMetadata<TParent>> WithAssign()
        {
            var ec = new AssignMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<IfMetadata<TParent>> WithCancel()
        {
            var ec = new CancelMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<IfMetadata<TParent>> WithForeach()
        {
            var ec = new ForeachMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<IfMetadata<TParent>> WithIf()
        {
            var ec = new IfMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<TParent> WithLog(string message)
        {
            var ec = new LogMetadata<IfMetadata<TParent>>();

            ec.WithMessage(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> WithLog(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<IfMetadata<TParent>>();

            ec.WithMessage(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> WithRaise(string messageName)
        {
            var ec = new RaiseMetadata<IfMetadata<TParent>>();

            ec.WithMessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> WithScript(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<IfMetadata<TParent>>();

            ec.WithAction(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<IfMetadata<TParent>> WithSendMessage()
        {
            var ec = new SendMessageMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        bool IIfMetadata.EvalIfCondition(dynamic data) => _eval?.Invoke(data) ?? false;

        IEnumerable<IExecutableContentMetadata> IIfMetadata.GetElseExecutableContent() => _else.GetExecutableContent();

        IEnumerable<Func<dynamic, bool>> IIfMetadata.GetElseIfConditions() => _elseIfs.Select(ei => ei.GetEvalCondition());

        IEnumerable<IEnumerable<IExecutableContentMetadata>> IIfMetadata.GetElseIfExecutableContent() => _elseIfs.Select(ei => ei.GetExecutableContent());

        IEnumerable<IExecutableContentMetadata> IIfMetadata.GetExecutableContent() => _executableContent;
    }
}
