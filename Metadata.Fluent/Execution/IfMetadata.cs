using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.Execution
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

        public IfMetadata<TParent> Condition(Func<dynamic, bool> condition)
        {
            _eval = condition;
            return this;
        }

        public ElseIfMetadata<IfMetadata<TParent>> ElseIf()
        {
            var elseif = new ElseIfMetadata<IfMetadata<TParent>>();

            _elseIfs.Add(elseif);

            elseif.Parent = this;

            elseif.MetadataId = $"{((IModelMetadata)this).MetadataId}.ElseIf[{_elseIfs.Count}]";

            return elseif;
        }

        public ElseMetadata<IfMetadata<TParent>> Else()
        {
            _else = new ElseMetadata<IfMetadata<TParent>>();

            _else.Parent = this;

            _else.MetadataId = $"{((IModelMetadata)this).MetadataId}.Else";

            return _else;
        }

        public AssignMetadata<IfMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<IfMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<IfMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<IfMetadata<TParent>> If()
        {
            var ec = new IfMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<IfMetadata<TParent>>();

            ec.Message(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<IfMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<IfMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public IfMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<IfMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<IfMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        internal QueryMetadata<IfMetadata<TParent>> Query()
        {
            var ec = new QueryMetadata<IfMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        bool IIfMetadata.EvalIfCondition(dynamic data) => _eval?.Invoke(data) ?? false;

        IEnumerable<IExecutableContentMetadata> IIfMetadata.GetElseExecutableContent() => _else.GetExecutableContent();

        IEnumerable<Func<dynamic, bool>> IIfMetadata.GetElseIfConditions() => _elseIfs.Select(ei => ei.GetEvalCondition());

        IEnumerable<IEnumerable<IExecutableContentMetadata>> IIfMetadata.GetElseIfExecutableContent() => _elseIfs.Select(ei => ei.GetExecutableContent());

        IEnumerable<IExecutableContentMetadata> IIfMetadata.GetExecutableContent() => _executableContent;
    }
}
