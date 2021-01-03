using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using System.Collections.Generic;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.States
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

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write(_datamodel, (o, w) => o.Serialize(w));
            writer.Write(_onEntry, (o, w) => o.Serialize(w));
            writer.Write(_onExit, (o, w) => o.Serialize(w));

            writer.WriteMany(_transitions, (o, w) => o.Serialize(w));
            writer.WriteMany(_stateChartInvokes, (o, w) => o.Serialize(w));
        }

        internal static AtomicStateMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var id = reader.ReadNullableString();

            var metadata = new AtomicStateMetadata<TParent>(id);

            metadata.MetadataId = reader.ReadNullableString();

            metadata._datamodel = reader.Read(DatamodelMetadata<AtomicStateMetadata<TParent>>.Deserialize,
                                              o => o.Parent = metadata);

            metadata._onEntry = reader.Read(OnEntryExitMetadata<AtomicStateMetadata<TParent>>.Deserialize,
                                            o => o.Parent = metadata);

            metadata._onExit = reader.Read(OnEntryExitMetadata<AtomicStateMetadata<TParent>>.Deserialize,
                                           o => o.Parent = metadata);

            metadata._transitions.AddRange(reader.ReadMany(TransitionMetadata<AtomicStateMetadata<TParent>>.Deserialize,
                                                             o => o.Parent = metadata));

            metadata._stateChartInvokes.AddRange(reader.ReadMany(InvokeStateChartMetadata<AtomicStateMetadata<TParent>>.Deserialize,
                                                                   o => o.Parent = metadata));

            return metadata;
        }

        protected override IStateMetadata _Parent => this.Parent;

        internal TParent Parent { get; set; }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public TParent _ => this.Parent;

        public AtomicStateMetadata<TParent> DataInit(string location, object value)
        {
            if (_datamodel == null)
            {
                _datamodel = this.Datamodel;
            }

            _datamodel.DataInit().Id(location).Value(value);

            return this;
        }

        public DatamodelMetadata<AtomicStateMetadata<TParent>> Datamodel
        {
            get
            {
                _datamodel = new DatamodelMetadata<AtomicStateMetadata<TParent>>();

                _datamodel.Parent = this;

                _datamodel.MetadataId = $"{((IModelMetadata)this).MetadataId}.Datamodel";

                return _datamodel;
            }
        }

        protected override IOnEntryExitMetadata GetOnEntry() => _onEntry;

        public OnEntryExitMetadata<AtomicStateMetadata<TParent>> OnEntry
        {
            get
            {
                _onEntry = new OnEntryExitMetadata<AtomicStateMetadata<TParent>>(true);

                _onEntry.Parent = this;

                _onEntry.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnEntry";

                return _onEntry;
            }
        }

        protected override IOnEntryExitMetadata GetOnExit() => _onExit;

        public OnEntryExitMetadata<AtomicStateMetadata<TParent>> OnExit
        {
            get
            {
                _onExit = new OnEntryExitMetadata<AtomicStateMetadata<TParent>>(false);

                _onExit.Parent = this;

                _onExit.MetadataId = $"{((IModelMetadata)this).MetadataId}.OnExit";

                return _onExit;
            }
        }

        protected override IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes() => _stateChartInvokes;

        public InvokeStateChartMetadata<AtomicStateMetadata<TParent>> InvokeStateChart
        {
            get
            {
                var invoke = new InvokeStateChartMetadata<AtomicStateMetadata<TParent>>();

                invoke.Parent = this;

                _stateChartInvokes.Add(invoke);

                invoke.MetadataId = $"{((IModelMetadata)this).MetadataId}.StateChartInvokes[{_stateChartInvokes.Count}]";

                return invoke;
            }
        }

        protected override IEnumerable<ITransitionMetadata> GetTransitions() => _transitions;

        public TransitionMetadata<AtomicStateMetadata<TParent>> Transition
        {
            get
            {
                var transition = new TransitionMetadata<AtomicStateMetadata<TParent>>();

                transition.Parent = this;

                _transitions.Add(transition);

                transition.MetadataId = $"{((IModelMetadata)this).MetadataId}.Transitions[{_transitions.Count}]";

                return transition;
            }
        }
    }
}
