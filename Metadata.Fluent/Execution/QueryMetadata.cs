using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Fluent.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class QueryMetadata<TParent> : ExecutableContentMetadata, IQueryMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _executableContent;
        private readonly List<ParamMetadata<QueryMetadata<TParent>>> _params;
        private string _resultLocation;
        private Func<dynamic, string> _targetGetter;
        private Func<dynamic, string> _typeGetter;

        internal QueryMetadata()
        {
            _executableContent = new List<ExecutableContentMetadata>();
            _params = new List<ParamMetadata<QueryMetadata<TParent>>>();
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public QueryMetadata<TParent> ResultLocation(string resultLocation)
        {
            _resultLocation = resultLocation;
            return this;
        }

        internal QueryMetadata<TParent> Target(string target)
        {
            _targetGetter = _ => target;
            return this;
        }

        internal QueryMetadata<TParent> Target(Func<dynamic, string> getter)
        {
            _targetGetter = getter;
            return this;
        }

        internal QueryMetadata<TParent> Type(string type)
        {
            _typeGetter = _ => type;
            return this;
        }

        internal QueryMetadata<TParent> Type(Func<dynamic, string> getter)
        {
            _typeGetter = getter;
            return this;
        }

        internal ParamMetadata<QueryMetadata<TParent>> Param(string name)
        {
            var param = new ParamMetadata<QueryMetadata<TParent>>(name);

            param.Parent = this;

            _params.Add(param);

            param.MetadataId = $"{((IModelMetadata)this).MetadataId}.Params[{_params.Count}]";

            return param;
        }

        public AssignMetadata<QueryMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public CancelMetadata<QueryMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<QueryMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public IfMetadata<QueryMetadata<TParent>> If()
        {
            var ec = new IfMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        public QueryMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<QueryMetadata<TParent>>();

            ec.Message(_ => message);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public QueryMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<QueryMetadata<TParent>>();

            ec.Message(getter);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public QueryMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<QueryMetadata<TParent>>();

            ec.MessageName(messageName);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        public QueryMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<QueryMetadata<TParent>>();

            ec.Action(action);

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.FinalizeExecutableContent[{_executableContent.Count}]";

            return this;
        }

        internal SendMessageMetadata<QueryMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        internal QueryMetadata<QueryMetadata<TParent>> Query()
        {
            var ec = new QueryMetadata<QueryMetadata<TParent>>();

            _executableContent.Add(ec);

            ec.Parent = this;

            ec.MetadataId = $"{((IModelMetadata)this).MetadataId}.ExecutableContent[{_executableContent.Count}]";

            return ec;
        }

        string IQueryMetadata.ResultLocation => _resultLocation;

        IReadOnlyDictionary<string, object> IQueryMetadata.GetParams(dynamic data) =>
            new ReadOnlyDictionary<string, object>(_params.ToDictionary(p => p.Name, p => p.GetValue(data)));

        string IQueryMetadata.GetTarget(dynamic data) => _targetGetter?.Invoke(data);

        string IQueryMetadata.GetType(dynamic data) => _typeGetter?.Invoke(data);

        IEnumerable<IExecutableContentMetadata> IQueryMetadata.GetExecutableContent() => _executableContent;
    }
}
