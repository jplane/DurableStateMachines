using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.DataManipulation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States
{
    public sealed class SequentialStateMetadata<TParent> : StateMetadata, ISequentialStateMetadata where TParent : IStateMetadata
    {
        private readonly List<StateMetadata> _states;
        private readonly List<InvokeStateChartMetadata<SequentialStateMetadata<TParent>>> _stateChartInvokes;
        private readonly List<TransitionMetadata<SequentialStateMetadata<TParent>>> _transitions;

        private TransitionMetadata<SequentialStateMetadata<TParent>> _initialTransition;
        private DatamodelMetadata<SequentialStateMetadata<TParent>> _datamodel;
        private OnEntryExitMetadata<SequentialStateMetadata<TParent>> _onEntry;
        private OnEntryExitMetadata<SequentialStateMetadata<TParent>> _onExit;

        internal SequentialStateMetadata(string id)
            : base(id)
        {
            _states = new List<StateMetadata>();
            _stateChartInvokes = new List<InvokeStateChartMetadata<SequentialStateMetadata<TParent>>>();
            _transitions = new List<TransitionMetadata<SequentialStateMetadata<TParent>>>();
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

        public TransitionMetadata<SequentialStateMetadata<TParent>> WithInitialTransition()
        {
            _initialTransition = new TransitionMetadata<SequentialStateMetadata<TParent>>();

            _initialTransition.Parent = this;

            _initialTransition.UniqueId = $"{((IModelMetadata)this).UniqueId}.InitialTransition";

            return _initialTransition;
        }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public DatamodelMetadata<SequentialStateMetadata<TParent>> WithDatamodel()
        {
            _datamodel = new DatamodelMetadata<SequentialStateMetadata<TParent>>();

            _datamodel.Parent = this;

            _datamodel.UniqueId = $"{((IModelMetadata)this).UniqueId}.Datamodel";

            return _datamodel;
        }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public OnEntryExitMetadata<SequentialStateMetadata<TParent>> WithOnEntry()
        {
            _onEntry = new OnEntryExitMetadata<SequentialStateMetadata<TParent>>(true);

            _onEntry.Parent = this;

            _onEntry.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnEntry";

            return _onEntry;
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<SequentialStateMetadata<TParent>> WithOnExit()
        {
            _onExit = new OnEntryExitMetadata<SequentialStateMetadata<TParent>>(false);

            _onExit.Parent = this;

            _onExit.UniqueId = $"{((IModelMetadata)this).UniqueId}.OnExit";

            return _onExit;
        }

        protected override IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes() => _stateChartInvokes;

        public InvokeStateChartMetadata<SequentialStateMetadata<TParent>> WithStateChartInvoke()
        {
            var invoke = new InvokeStateChartMetadata<SequentialStateMetadata<TParent>>();

            invoke.Parent = this;

            _stateChartInvokes.Add(invoke);

            invoke.UniqueId = $"{((IModelMetadata)this).UniqueId}.StateChartInvokes[{_stateChartInvokes.Count}]";

            return invoke;
        }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => _transitions;

        public TransitionMetadata<SequentialStateMetadata<TParent>> WithTransition()
        {
            var transition = new TransitionMetadata<SequentialStateMetadata<TParent>>();

            transition.Parent = this;

            _transitions.Add(transition);

            transition.UniqueId = $"{((IModelMetadata)this).UniqueId}.Transitions[{_transitions.Count}]";

            return transition;
        }

        public AtomicStateMetadata<SequentialStateMetadata<TParent>> WithAtomicState(string id)
        {
            return WithState<AtomicStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public SequentialStateMetadata<SequentialStateMetadata<TParent>> WithSequentialState(string id)
        {
            return WithState<SequentialStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public ParallelStateMetadata<SequentialStateMetadata<TParent>> WithParallelState(string id)
        {
            return WithState<ParallelStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public FinalStateMetadata<SequentialStateMetadata<TParent>> WithFinalState(string id)
        {
            return WithState<FinalStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public HistoryStateMetadata<SequentialStateMetadata<TParent>> WithHistoryState(string id)
        {
            return WithState<HistoryStateMetadata<SequentialStateMetadata<TParent>>>(id);
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

        ITransitionMetadata ISequentialStateMetadata.GetInitialTransition()
        {
            if (_initialTransition != null)
            {
                return _initialTransition;
            }
            else if (_states.Count > 0)
            {
                return new TransitionMetadata<SequentialStateMetadata<TParent>>()
                                        .WithTarget(((IStateMetadata)_states[0]).Id);
            }
            else
            {
                return null;
            }
        }

        IEnumerable<IStateMetadata> ISequentialStateMetadata.GetStates() => _states;
    }
}
