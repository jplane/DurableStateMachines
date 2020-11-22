using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.DataManipulation;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States
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

        public OnEntryExitMetadata<FinalStateMetadata<TParent>> WithOnEntry()
        {
            _onEntry = new OnEntryExitMetadata<FinalStateMetadata<TParent>>(true);

            _onEntry.Parent = this;

            _onEntry.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnEntry";

            return _onEntry;
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<FinalStateMetadata<TParent>> WithOnExit()
        {
            _onExit = new OnEntryExitMetadata<FinalStateMetadata<TParent>>(false);

            _onExit.Parent = this;

            _onExit.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnExit";

            return _onExit;
        }

        public ContentMetadata<FinalStateMetadata<TParent>> WithContent()
        {
            _content = new ContentMetadata<FinalStateMetadata<TParent>>();

            _content.Parent = this;

            _content.UniqueId = $"{((IModelMetadata)this).UniqueId}.Content";

            return _content;
        }

        public ParamMetadata<FinalStateMetadata<TParent>> WithParam()
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
