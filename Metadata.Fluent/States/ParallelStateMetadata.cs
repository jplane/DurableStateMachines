using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class ParallelStateMetadata<TParent> : StateMetadata, IParallelStateMetadata where TParent : IStateMetadata
    {
        private readonly List<StateMetadata> _states;
        private readonly List<TransitionMetadata<ParallelStateMetadata<TParent>>> _transitions;

        private DatamodelMetadata<ParallelStateMetadata<TParent>> _datamodel;
        private OnEntryExitMetadata<ParallelStateMetadata<TParent>> _onEntry;
        private OnEntryExitMetadata<ParallelStateMetadata<TParent>> _onExit;

        internal ParallelStateMetadata(string id)
            : base(id)
        {
            _states = new List<StateMetadata>();
            _transitions = new List<TransitionMetadata<ParallelStateMetadata<TParent>>>();
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        internal override IEnumerable<string> StateNames
        {
            get => new[] { ((IStateMetadata)this).Id }.Concat(_states.SelectMany(s => s.StateNames));
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public DatamodelMetadata<ParallelStateMetadata<TParent>> Datamodel()
        {
            _datamodel = new DatamodelMetadata<ParallelStateMetadata<TParent>>();

            _datamodel.Parent = this;

            _datamodel.UniqueId = $"{((IModelMetadata)this).UniqueId}.Datamodel";

            return _datamodel;
        }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public OnEntryExitMetadata<ParallelStateMetadata<TParent>> OnEntry()
        {
            _onEntry = new OnEntryExitMetadata<ParallelStateMetadata<TParent>>(true);

            _onEntry.Parent = this;

            _onEntry.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnEntry";

            return _onEntry;
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<ParallelStateMetadata<TParent>> OnExit()
        {
            _onExit = new OnEntryExitMetadata<ParallelStateMetadata<TParent>>(false);

            _onExit.Parent = this;

            _onExit.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnExit";

            return _onExit;
        }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => _transitions;

        public TransitionMetadata<ParallelStateMetadata<TParent>> Transition()
        {
            var transition = new TransitionMetadata<ParallelStateMetadata<TParent>>();

            transition.Parent = this;

            _transitions.Add(transition);

            transition.UniqueId = $"{((IModelMetadata)this).UniqueId}.Transitions[{_transitions.Count}]";

            return transition;
        }

        public AtomicStateMetadata<ParallelStateMetadata<TParent>> AtomicState(string id)
        {
            return WithState<AtomicStateMetadata<ParallelStateMetadata<TParent>>>(id);
        }

        public SequentialStateMetadata<ParallelStateMetadata<TParent>> SequentialState(string id)
        {
            return WithState<SequentialStateMetadata<ParallelStateMetadata<TParent>>>(id);
        }

        public ParallelStateMetadata<ParallelStateMetadata<TParent>> ParallelState(string id)
        {
            return WithState<ParallelStateMetadata<ParallelStateMetadata<TParent>>>(id);
        }

        private TStateMetadata WithState<TStateMetadata>(string id) where TStateMetadata : StateMetadata
        {
            var method = typeof(TStateMetadata).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                                                               null,
                                                               new[] { typeof(string) },
                                                               null);

            Debug.Assert(method != null);

            var state = (dynamic) method.Invoke(new[] { id });

            state.Parent = this;

            _states.Add(state);

            return state;
        }

        IEnumerable<IStateMetadata> IParallelStateMetadata.GetStates() => _states;
    }
}
