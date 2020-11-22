using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.DataManipulation;
using System.Collections.Generic;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States
{
    public sealed class AtomicStateMetadata<TParent> : StateMetadata, IAtomicStateMetadata where TParent : IStateMetadata
    {
        private readonly List<InvokeStateChartMetadata<AtomicStateMetadata<TParent>>> _stateChartInvokes;
        private readonly List<TransitionMetadata<AtomicStateMetadata<TParent>>> _transitions;

        private DatamodelMetadata<AtomicStateMetadata<TParent>> _datamodel;
        private OnEntryExitMetadata<AtomicStateMetadata<TParent>> _onEntry;
        private OnEntryExitMetadata<AtomicStateMetadata<TParent>> _onExit;

        internal AtomicStateMetadata(string id)
            : base(id)
        {
            _stateChartInvokes = new List<InvokeStateChartMetadata<AtomicStateMetadata<TParent>>>();
            _transitions = new List<TransitionMetadata<AtomicStateMetadata<TParent>>>();
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public TParent Attach()
        {
            return this.Parent;
        }

        public DatamodelMetadata<AtomicStateMetadata<TParent>> WithDatamodel()
        {
            _datamodel = new DatamodelMetadata<AtomicStateMetadata<TParent>>();

            _datamodel.Parent = this;

            _datamodel.UniqueId = $"{((IModelMetadata)this).UniqueId}.Datamodel";

            return _datamodel;
        }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public OnEntryExitMetadata<AtomicStateMetadata<TParent>> WithOnEntry()
        {
            _onEntry = new OnEntryExitMetadata<AtomicStateMetadata<TParent>>(true);

            _onEntry.Parent = this;

            _onEntry.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnEntry";

            return _onEntry;
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<AtomicStateMetadata<TParent>> WithOnExit()
        {
            _onExit = new OnEntryExitMetadata<AtomicStateMetadata<TParent>>(false);

            _onExit.Parent = this;

            _onExit.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnExit";

            return _onExit;
        }

        protected override IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes() => _stateChartInvokes;

        public InvokeStateChartMetadata<AtomicStateMetadata<TParent>> WithStateChartInvoke()
        {
            var invoke = new InvokeStateChartMetadata<AtomicStateMetadata<TParent>>();

            invoke.Parent = this;

            _stateChartInvokes.Add(invoke);

            invoke.UniqueId = $"{((IModelMetadata)this).UniqueId}.StateChartInvokes[{_stateChartInvokes.Count}]";

            return invoke;
        }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => _transitions;

        public TransitionMetadata<AtomicStateMetadata<TParent>> WithTransition()
        {
            var transition = new TransitionMetadata<AtomicStateMetadata<TParent>>();

            transition.Parent = this;

            _transitions.Add(transition);

            transition.UniqueId = $"{((IModelMetadata)this).UniqueId}.Transitions[{_transitions.Count}]";

            return transition;
        }
    }
}
