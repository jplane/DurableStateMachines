using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using StateChartsDotNet.Metadata.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class StateChart : StateMetadata, IStateChartMetadata
    {
        private readonly List<InvokeStateChartMetadata<StateChart>> _stateChartInvokes;
        private readonly List<TransitionMetadata<StateChart>> _transitions;

        private DatamodelMetadata<StateChart> _datamodel;
        private bool _failFast;
        private Databinding _databinding;
        private TransitionMetadata<StateChart> _initialTransition;
        private ScriptMetadata<StateChart> _script;

        private readonly List<StateMetadata> _states = new List<StateMetadata>();

        private StateChart(string name)
            : base(name)
        {
            _databinding = Common.Model.Databinding.Early;
            _stateChartInvokes = new List<InvokeStateChartMetadata<StateChart>>();
            _transitions = new List<TransitionMetadata<StateChart>>();
        }

        protected override IStateMetadata _Parent => null;

        public async Task<(string, string)> ToStringAsync(CancellationToken cancelToken = default)
        {
            using var stream = new MemoryStream();

            var metadataType = await this.SerializeAsync(stream, cancelToken);

            Debug.Assert(!string.IsNullOrWhiteSpace(metadataType));

            stream.Position = 0;

            var bytes = Convert.ToBase64String(stream.ToArray());

            return (metadataType, bytes);
        }

        public static Task<IStateChartMetadata> FromStringAsync(string content,
                                                                CancellationToken cancelToken = default)
        {
            content.CheckArgNull(nameof(content));

            var bytes = Convert.FromBase64String(content);

            using var stream = new MemoryStream(bytes);

            return DeserializeAsync(stream, cancelToken);
        }

        public Task<string> SerializeAsync(Stream stream, CancellationToken cancelToken = default)
        {
            stream.CheckArgNull(nameof(stream));

            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

            Serialize(writer);

            return Task.FromResult("fluent");
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.Write((int)_databinding);
            writer.Write(_failFast);

            writer.Write(_datamodel, (o, w) => o.Serialize(w));
            writer.Write(_initialTransition, (o, w) => o.Serialize(w));
            writer.Write(_script, (o, w) => o.Serialize(w));

            writer.WriteMany(_states, (o, w) => o.Serialize(w));
            writer.WriteMany(_transitions, (o, w) => o.Serialize(w));
            writer.WriteMany(_stateChartInvokes, (o, w) => o.Serialize(w));
        }

        public static Task<IStateChartMetadata> DeserializeAsync(Stream stream,
                                                                 CancellationToken cancelToken = default)
        {
            stream.CheckArgNull(nameof(stream));

            using var reader = new BinaryReader(stream, Encoding.UTF8, true);

            var statechart = Deserialize(reader);

            return Task.FromResult((IStateChartMetadata) statechart);
        }

        internal static StateChart Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            reader.ReadNullableString();    // aqtn, can safely ignore

            var name = reader.ReadNullableString();

            var statechart = new StateChart(name);

            statechart.MetadataId = reader.ReadNullableString();
            statechart._databinding = (Databinding)reader.ReadInt32();
            statechart._failFast = reader.ReadBoolean();

            statechart._datamodel = reader.Read(DatamodelMetadata<StateChart>.Deserialize,
                                                o => o.Parent = statechart);

            statechart._initialTransition = reader.Read(TransitionMetadata<StateChart>.Deserialize,
                                                        o => o.Parent = statechart);

            statechart._script = reader.Read(ScriptMetadata<StateChart>.Deserialize,
                                             o => o.Parent = statechart);

            statechart._states.AddRange(StateMetadata.DeserializeMany(reader, statechart));

            statechart._transitions.AddRange(reader.ReadMany(TransitionMetadata<StateChart>.Deserialize,
                                                             o => o.Parent = statechart));

            statechart._stateChartInvokes.AddRange(reader.ReadMany(InvokeStateChartMetadata<StateChart>.Deserialize,
                                                                   o => o.Parent = statechart));

            return statechart;
        }

        public override StateType Type => StateType.Root;

        public override string MetadataId 
        {
            get => this.Id; set => throw new NotSupportedException(); 
        }

        internal override IEnumerable<string> StateNames
        {
            get => new[] { ((IStateMetadata)this).Id }.Concat(_states.SelectMany(s => s.StateNames));
        }

        public static StateChart Define(string name)
        {
            return new StateChart(name);
        }

        public StateChart FailFast(bool failFast)
        {
            _failFast = failFast;
            return this;
        }

        public StateChart Databinding(Databinding databinding)
        {
            _databinding = databinding;
            return this;
        }

        public StateChart InitialState(string initialState)
        {
            _initialTransition = new TransitionMetadata<StateChart>().Target(initialState);

            _initialTransition.Parent = this;

            _initialTransition.MetadataId = $"{((IModelMetadata)this).MetadataId}.InitialTransition";

            return this;
        }

        public StateChart Execute(Expression<Action<IDictionary<string, object>>> action)
        {
            _script = new ScriptMetadata<StateChart>();

            _script.Parent = this;

            _script.MetadataId = $"{((IModelMetadata)this).MetadataId}.Script";

            _script.Action(action);

            return this;
        }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public StateChart DataInit(string location, object value)
        {
            if (_datamodel == null)
            {
                _datamodel = this.Datamodel;
            }

            _datamodel.DataInit().Id(location).Value(value);

            return this;
        }

        public DatamodelMetadata<StateChart> Datamodel
        {
            get
            {
                _datamodel = new DatamodelMetadata<StateChart>();

                _datamodel.Parent = this;

                _datamodel.MetadataId = $"{((IModelMetadata)this).MetadataId}.Datamodel";

                return _datamodel;
            }
        }

        public StateMetadata<StateChart> State(string id)
        {
            return WithState<StateMetadata<StateChart>>(id);
        }

        public ParallelStateMetadata<StateChart> ParallelState(string id)
        {
            return WithState<ParallelStateMetadata<StateChart>>(id);
        }

        public FinalStateMetadata<StateChart> FinalState(string id)
        {
            return WithState<FinalStateMetadata<StateChart>>(id);
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

        bool IStateChartMetadata.FailFast => _failFast;

        Databinding IStateChartMetadata.Databinding => _databinding;

        IScriptMetadata IStateChartMetadata.GetScript() => _script;

        protected override ITransitionMetadata GetInitialTransition()
        {
            if (_initialTransition != null)
            {
                return _initialTransition;
            }
            else
            {
                this.InitialState(((IStateMetadata)_states[0]).Id);

                Debug.Assert(_initialTransition != null);

                return _initialTransition;
            }
        }

        protected override IEnumerable<IStateMetadata> GetStates() => _states;
    }
}
