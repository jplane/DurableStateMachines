using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.DataManipulation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class FinalStateMetadata<TParent> : StateMetadata, IFinalStateMetadata where TParent : IStateMetadata
    {
        private Func<dynamic, object> _contentGetter;
        private OnEntryExitMetadata<FinalStateMetadata<TParent>> _onEntry;
        private OnEntryExitMetadata<FinalStateMetadata<TParent>> _onExit;

        private readonly List<ParamMetadata<FinalStateMetadata<TParent>>> _params =
            new List<ParamMetadata<FinalStateMetadata<TParent>>>();

        internal FinalStateMetadata(string id)
            : base(id)
        {
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public TParent Attach()
        {
            return this.Parent;
        }

        public OnEntryExitMetadata<FinalStateMetadata<TParent>> OnEntry()
        {
            _onEntry = new OnEntryExitMetadata<FinalStateMetadata<TParent>>(true);

            _onEntry.Parent = this;

            _onEntry.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnEntry";

            return _onEntry;
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<FinalStateMetadata<TParent>> OnExit()
        {
            _onExit = new OnEntryExitMetadata<FinalStateMetadata<TParent>>(false);

            _onExit.Parent = this;

            _onExit.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnExit";

            return _onExit;
        }

        public FinalStateMetadata<TParent> Content(Func<dynamic, object> getter)
        {
            _contentGetter = getter;
            return this;
        }

        public ParamMetadata<FinalStateMetadata<TParent>> Param(string name)
        {
            var param = new ParamMetadata<FinalStateMetadata<TParent>>(name);

            param.Parent = this;

            _params.Add(param);

            param.UniqueId = $"{((IModelMetadata)this).UniqueId}.Params[{_params.Count}]";

            return param;
        }

        object IFinalStateMetadata.GetContent(dynamic data) => _contentGetter?.Invoke(data);

        IReadOnlyDictionary<string, Func<dynamic, object>> IFinalStateMetadata.GetParams() =>
            new ReadOnlyDictionary<string, Func<dynamic, object>>(
                _params.ToDictionary(p => p.Name, p => (Func<dynamic, object>)p.GetValue));
    }
}
