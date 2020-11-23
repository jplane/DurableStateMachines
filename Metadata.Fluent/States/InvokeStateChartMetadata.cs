using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using StateChartsDotNet.Metadata.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class InvokeStateChartMetadata<TParent> : IInvokeStateChartMetadata where TParent : IModelMetadata
    {
        private readonly List<ExecutableContentMetadata> _finalizeExecutableContent;
        private readonly List<ParamMetadata<InvokeStateChartMetadata<TParent>>> _params;

        private bool _autoForward;
        private string _id;
        private string _idLocation;
        private Func<dynamic, object> _contentGetter;
        private Func<dynamic, object> _typeGetter;

        internal InvokeStateChartMetadata()
        {
            _finalizeExecutableContent = new List<ExecutableContentMetadata>();
            _params = new List<ParamMetadata<InvokeStateChartMetadata<TParent>>>();
        }

        internal TParent Parent { get; set; }

        internal string UniqueId { private get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public InvokeStateChartMetadata<TParent> Autoforward(bool autoforward)
        {
            _autoForward = autoforward;
            return this;
        }

        public InvokeStateChartMetadata<TParent> Id(string id)
        {
            _id = id;
            _idLocation = null;
            return this;
        }

        public InvokeStateChartMetadata<TParent> IdLocation(string idLocation)
        {
            _id = null;
            _idLocation = idLocation;
            return this;
        }

        public InvokeStateChartMetadata<TParent> Content(Func<dynamic, object> getter)
        {
            _contentGetter = getter;
            return this;
        }

        public InvokeStateChartMetadata<TParent> Type(Func<dynamic, object> getter)
        {
            _typeGetter = getter;
            return this;
        }

        public ParamMetadata<InvokeStateChartMetadata<TParent>> Param(string name)
        {
            var param = new ParamMetadata<InvokeStateChartMetadata<TParent>>(name);

            param.Parent = this;

            _params.Add(param);

            param.UniqueId = $"{((IModelMetadata)this).UniqueId}.Params[{_params.Count}]";

            return param;
        }

        public AssignMetadata<InvokeStateChartMetadata<TParent>> Assign()
        {
            var ec = new AssignMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public CancelMetadata<InvokeStateChartMetadata<TParent>> Cancel()
        {
            var ec = new CancelMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public ForeachMetadata<InvokeStateChartMetadata<TParent>> Foreach()
        {
            var ec = new ForeachMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public IfMetadata<InvokeStateChartMetadata<TParent>> If()
        {
            var ec = new IfMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        public InvokeStateChartMetadata<TParent> Log(string message)
        {
            var ec = new LogMetadata<InvokeStateChartMetadata<TParent>>();

            ec.Message(_ => message);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        public InvokeStateChartMetadata<TParent> Log(Func<dynamic, string> getter)
        {
            var ec = new LogMetadata<InvokeStateChartMetadata<TParent>>();

            ec.Message(getter);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        public InvokeStateChartMetadata<TParent> Raise(string messageName)
        {
            var ec = new RaiseMetadata<InvokeStateChartMetadata<TParent>>();

            ec.MessageName(messageName);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        public InvokeStateChartMetadata<TParent> Execute(Action<dynamic> action)
        {
            var ec = new ScriptMetadata<InvokeStateChartMetadata<TParent>>();

            ec.Action(action);

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.FinalizeExecutableContent[{_finalizeExecutableContent.Count}]";

            return this;
        }

        public SendMessageMetadata<InvokeStateChartMetadata<TParent>> SendMessage()
        {
            var ec = new SendMessageMetadata<InvokeStateChartMetadata<TParent>>();

            _finalizeExecutableContent.Add(ec);

            ec.Parent = this;

            ec.UniqueId = $"{((IModelMetadata)this).UniqueId}.ExecutableContent[{_finalizeExecutableContent.Count}]";

            return ec;
        }

        bool IInvokeStateChartMetadata.Autoforward => _autoForward;

        string IInvokeStateChartMetadata.Id => _id;

        string IInvokeStateChartMetadata.IdLocation => _idLocation;

        string IModelMetadata.UniqueId => this.UniqueId;

        IEnumerable<IExecutableContentMetadata> IInvokeStateChartMetadata.GetFinalizeExecutableContent() => _finalizeExecutableContent;

        string IInvokeStateChartMetadata.GetType(dynamic data) => _typeGetter?.Invoke(data);

        object IInvokeStateChartMetadata.GetContent(dynamic data) => _contentGetter?.Invoke(data);

        IReadOnlyDictionary<string, Func<dynamic, object>> IInvokeStateChartMetadata.GetParams() =>
            new ReadOnlyDictionary<string, Func<dynamic, object>>(
                _params.ToDictionary(p => p.Name, p => (Func<dynamic, object>)p.GetValue));

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
