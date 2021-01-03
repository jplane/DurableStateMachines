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

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write(_datamodel, (o, w) => o.Serialize(w));
            writer.Write(_onEntry, (o, w) => o.Serialize(w));
            writer.Write(_onExit, (o, w) => o.Serialize(w));

            writer.WriteMany(_states, (o, w) => o.Serialize(w));
            writer.WriteMany(_transitions, (o, w) => o.Serialize(w));
        }

        internal static ParallelStateMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var id = reader.ReadNullableString();

            var metadata = new ParallelStateMetadata<TParent>(id);

            metadata.MetadataId = reader.ReadNullableString();

            metadata._datamodel = reader.Read(DatamodelMetadata<ParallelStateMetadata<TParent>>.Deserialize,
                                              o => o.Parent = metadata);

            metadata._onEntry = reader.Read(OnEntryExitMetadata<ParallelStateMetadata<TParent>>.Deserialize,
                                            o => o.Parent = metadata);

            metadata._onExit = reader.Read(OnEntryExitMetadata<ParallelStateMetadata<TParent>>.Deserialize,
                                           o => o.Parent = metadata);

            metadata._states.AddRange(StateMetadata.DeserializeMany(reader, metadata));

            metadata._transitions.AddRange(reader.ReadMany(TransitionMetadata<ParallelStateMetadata<TParent>>.Deserialize,
                                                             o => o.Parent = metadata));

            return metadata;
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        internal override IEnumerable<string> StateNames
        {
            get => new[] { ((IStateMetadata)this).Id }.Concat(_states.SelectMany(s => s.StateNames));
        }

        public TParent _ => this.Parent;

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public ParallelStateMetadata<TParent> DataInit(string location, object value)
        {
            if (_datamodel == null)
            {
                _datamodel = this.Datamodel;
            }

            _datamodel.DataInit().Id(location).Value(value);

            return this;
        }

        public DatamodelMetadata<ParallelStateMetadata<TParent>> Datamodel
        {
            get
            {
                _datamodel = new DatamodelMetadata<ParallelStateMetadata<TParent>>();

                _datamodel.Parent = this;

                _datamodel.MetadataId = $"{((IModelMetadata)this).MetadataId}.Datamodel";

                return _datamodel;
            }
        }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public OnEntryExitMetadata<ParallelStateMetadata<TParent>> OnEntry
        {
            get
            {
                _onEntry = new OnEntryExitMetadata<ParallelStateMetadata<TParent>>(true);

                _onEntry.Parent = this;

                _onEntry.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnEntry";

                return _onEntry;
            }
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<ParallelStateMetadata<TParent>> OnExit
        {
            get
            {
                _onExit = new OnEntryExitMetadata<ParallelStateMetadata<TParent>>(false);

                _onExit.Parent = this;

                _onExit.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnExit";

                return _onExit;
            }
        }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => _transitions;

        public TransitionMetadata<ParallelStateMetadata<TParent>> Transition
        {
            get
            {
                var transition = new TransitionMetadata<ParallelStateMetadata<TParent>>();

                transition.Parent = this;

                _transitions.Add(transition);

                transition.MetadataId = $"{((IModelMetadata)this).MetadataId}.Transitions[{_transitions.Count}]";

                return transition;
            }
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

            state.MetadataId = $"{this.MetadataId}.States[{_states.Count}]";

            return state;
        }

        IEnumerable<IStateMetadata> IParallelStateMetadata.GetStates() => _states;
    }
}
