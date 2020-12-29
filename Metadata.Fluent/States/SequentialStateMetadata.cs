using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StateChartsDotNet.Metadata.Fluent.States
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

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write(_datamodel, (o, w) => o.Serialize(w));
            writer.Write(_initialTransition, (o, w) => o.Serialize(w));
            writer.Write(_onEntry, (o, w) => o.Serialize(w));
            writer.Write(_onExit, (o, w) => o.Serialize(w));

            writer.WriteMany(_states, (o, w) => o.Serialize(w));
            writer.WriteMany(_transitions, (o, w) => o.Serialize(w));
            writer.WriteMany(_stateChartInvokes, (o, w) => o.Serialize(w));
        }

        internal static SequentialStateMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var id = reader.ReadString();

            var metadata = new SequentialStateMetadata<TParent>(id);

            metadata.MetadataId = reader.ReadString();

            metadata._datamodel = reader.Read(DatamodelMetadata<SequentialStateMetadata<TParent>>.Deserialize,
                                              o => o.Parent = metadata);

            metadata._initialTransition = reader.Read(TransitionMetadata<SequentialStateMetadata<TParent>>.Deserialize,
                                                      o => o.Parent = metadata);

            metadata._onEntry = reader.Read(OnEntryExitMetadata<SequentialStateMetadata<TParent>>.Deserialize,
                                            o => o.Parent = metadata);

            metadata._onExit = reader.Read(OnEntryExitMetadata<SequentialStateMetadata<TParent>>.Deserialize,
                                           o => o.Parent = metadata);

            metadata._states.AddRange(StateMetadata.DeserializeMany(reader, metadata));

            metadata._transitions.AddRange(reader.ReadMany(TransitionMetadata<SequentialStateMetadata<TParent>>.Deserialize,
                                                             o => o.Parent = metadata));

            metadata._stateChartInvokes.AddRange(reader.ReadMany(InvokeStateChartMetadata<SequentialStateMetadata<TParent>>.Deserialize,
                                                                   o => o.Parent = metadata));

            return metadata;
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

        public TransitionMetadata<SequentialStateMetadata<TParent>> InitialTransition()
        {
            _initialTransition = new TransitionMetadata<SequentialStateMetadata<TParent>>();

            _initialTransition.Parent = this;

            _initialTransition.MetadataId = $"{((IModelMetadata)this).MetadataId}.InitialTransition";

            return _initialTransition;
        }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public DatamodelMetadata<SequentialStateMetadata<TParent>> Datamodel()
        {
            _datamodel = new DatamodelMetadata<SequentialStateMetadata<TParent>>();

            _datamodel.Parent = this;

            _datamodel.MetadataId = $"{((IModelMetadata)this).MetadataId}.Datamodel";

            return _datamodel;
        }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public OnEntryExitMetadata<SequentialStateMetadata<TParent>> OnEntry()
        {
            _onEntry = new OnEntryExitMetadata<SequentialStateMetadata<TParent>>(true);

            _onEntry.Parent = this;

            _onEntry.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnEntry";

            return _onEntry;
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<SequentialStateMetadata<TParent>> OnExit()
        {
            _onExit = new OnEntryExitMetadata<SequentialStateMetadata<TParent>>(false);

            _onExit.Parent = this;

            _onExit.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnExit";

            return _onExit;
        }

        protected override IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes() => _stateChartInvokes;

        public InvokeStateChartMetadata<SequentialStateMetadata<TParent>> InvokeStateChart()
        {
            var invoke = new InvokeStateChartMetadata<SequentialStateMetadata<TParent>>();

            invoke.Parent = this;

            _stateChartInvokes.Add(invoke);

            invoke.MetadataId = $"{((IModelMetadata)this).MetadataId}.StateChartInvokes[{_stateChartInvokes.Count}]";

            return invoke;
        }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => _transitions;

        public TransitionMetadata<SequentialStateMetadata<TParent>> Transition()
        {
            var transition = new TransitionMetadata<SequentialStateMetadata<TParent>>();

            transition.Parent = this;

            _transitions.Add(transition);

            transition.MetadataId = $"{((IModelMetadata)this).MetadataId}.Transitions[{_transitions.Count}]";

            return transition;
        }

        public AtomicStateMetadata<SequentialStateMetadata<TParent>> AtomicState(string id)
        {
            return WithState<AtomicStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public SequentialStateMetadata<SequentialStateMetadata<TParent>> SequentialState(string id)
        {
            return WithState<SequentialStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public ParallelStateMetadata<SequentialStateMetadata<TParent>> ParallelState(string id)
        {
            return WithState<ParallelStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public FinalStateMetadata<SequentialStateMetadata<TParent>> FinalState(string id)
        {
            return WithState<FinalStateMetadata<SequentialStateMetadata<TParent>>>(id);
        }

        public HistoryStateMetadata<SequentialStateMetadata<TParent>> HistoryState(string id)
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

            state.MetadataId = $"{this.MetadataId}.States[{_states.Count}]";

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
                                        .Target(((IStateMetadata)_states[0]).Id);
            }
            else
            {
                return null;
            }
        }

        IEnumerable<IStateMetadata> ISequentialStateMetadata.GetStates() => _states;
    }
}
