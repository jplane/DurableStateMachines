using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Fluent.Data;
using StateChartsDotNet.Metadata.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace StateChartsDotNet.Metadata.Fluent.States
{
    public sealed class StateChart : StateMetadata, IRootStateMetadata
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

        public override string UniqueId 
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

            _initialTransition.UniqueId = $"{((IModelMetadata)this).UniqueId}.InitialTransition";

            return this;
        }

        public ScriptMetadata<StateChart> Execute()
        {
            _script = new ScriptMetadata<StateChart>();

            _script.Parent = this;

            _script.UniqueId = $"{((IModelMetadata)this).UniqueId}.Script";

            return _script;
        }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public DatamodelMetadata<StateChart> Datamodel()
        {
            _datamodel = new DatamodelMetadata<StateChart>();

            _datamodel.Parent = this;

            _datamodel.UniqueId = $"{((IModelMetadata)this).UniqueId}.Datamodel";

            return _datamodel;
        }

        public AtomicStateMetadata<StateChart> AtomicState(string id)
        {
            return WithState<AtomicStateMetadata<StateChart>>(id);
        }

        public SequentialStateMetadata<StateChart> SequentialState(string id)
        {
            return WithState<SequentialStateMetadata<StateChart>>(id);
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

            state.UniqueId = $"{this.UniqueId}.States[{_states.Count}]";

            return state;
        }

        bool IRootStateMetadata.FailFast => _failFast;

        Databinding IRootStateMetadata.Databinding => _databinding;

        ITransitionMetadata IRootStateMetadata.GetInitialTransition()
        {
            if (_initialTransition != null)
            {
                return _initialTransition;
            }
            else if (_states.Count > 0)
            {
                return new TransitionMetadata<StateChart>()
                                    .Target(((IStateMetadata)_states[0]).Id);
            }
            else
            {
                return null;
            }
        }

        IScriptMetadata IRootStateMetadata.GetScript() => _script;

        IEnumerable<IStateMetadata> IRootStateMetadata.GetStates() => _states;
    }
}
