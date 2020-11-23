using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class HistoryStateMetadata<TParent> : StateMetadata, IHistoryStateMetadata where TParent : IStateMetadata
    {
        private TransitionMetadata<HistoryStateMetadata<TParent>> _transition;
        private HistoryType _type;

        internal HistoryStateMetadata(string id)
            : base(id)
        {
            _type = HistoryType.Deep;
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => new[] { _transition };

        public TParent Attach()
        {
            return this.Parent;
        }

        public TransitionMetadata<HistoryStateMetadata<TParent>> Transition()
        {
            _transition = new TransitionMetadata<HistoryStateMetadata<TParent>>();

            _transition.Parent = this;

            _transition.UniqueId = $"{((IModelMetadata)this).UniqueId}.Transition";

            return _transition;
        }

        public HistoryStateMetadata<TParent> Type(HistoryType type)
        {
            _type = type;

            return this;
        }

        HistoryType IHistoryStateMetadata.Type => _type;
    }
}
