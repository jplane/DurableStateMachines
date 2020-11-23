using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.DataManipulation;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class FinalStateMetadata<TParent> : StateMetadata, IFinalStateMetadata where TParent : IStateMetadata
    {
        private ContentMetadata<FinalStateMetadata<TParent>> _content;
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

        public ContentMetadata<FinalStateMetadata<TParent>> Content()
        {
            _content = new ContentMetadata<FinalStateMetadata<TParent>>();

            _content.Parent = this;

            _content.UniqueId = $"{((IModelMetadata)this).UniqueId}.Content";

            return _content;
        }

        public ParamMetadata<FinalStateMetadata<TParent>> Param()
        {
            var param = new ParamMetadata<FinalStateMetadata<TParent>>();

            param.Parent = this;

            _params.Add(param);

            param.UniqueId = $"{((IModelMetadata)this).UniqueId}.Params[{_params.Count}]";

            return param;
        }

        IContentMetadata IFinalStateMetadata.GetContent() => _content;

        IEnumerable<IParamMetadata> IFinalStateMetadata.GetParams() => _params;
    }
}
