using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.DataManipulation;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States
{
    public sealed class RootStateMetadata : StateMetadata, IRootStateMetadata
    {
        private readonly List<InvokeStateChartMetadata<RootStateMetadata>> _stateChartInvokes;
        private readonly List<TransitionMetadata<RootStateMetadata>> _transitions;

        private DatamodelMetadata<RootStateMetadata> _datamodel;
        private Databinding _databinding;
        private TransitionMetadata<RootStateMetadata> _initialTransition;
        private ScriptMetadata<RootStateMetadata> _script;

        private readonly List<StateMetadata> _states = new List<StateMetadata>();

        private RootStateMetadata(string name)
            : base(name)
        {
            _databinding = Databinding.Early;
            _stateChartInvokes = new List<InvokeStateChartMetadata<RootStateMetadata>>();
            _transitions = new List<TransitionMetadata<RootStateMetadata>>();
        }

        protected override IStateMetadata _Parent => null;

        internal override IEnumerable<string> StateNames
        {
            get => new[] { ((IStateMetadata)this).Id }.Concat(_states.SelectMany(s => s.StateNames));
        }

        public static RootStateMetadata Create(string name)
        {
            return new RootStateMetadata(name);
        }

        public RootStateMetadata WithDatabinding(Databinding databinding)
        {
            _databinding = databinding;
            return this;
        }

        public RootStateMetadata WithInitialState(string initialState)
        {
            _initialTransition = new TransitionMetadata<RootStateMetadata>().WithTarget(initialState);

            _initialTransition.Parent = this;

            _initialTransition.UniqueId = $"{((IModelMetadata)this).UniqueId}.InitialTransition";

            return this;
        }

        public ScriptMetadata<RootStateMetadata> WithScript()
        {
            _script = new ScriptMetadata<RootStateMetadata>();

            _script.Parent = this;

            _script.UniqueId = $"{((IModelMetadata)this).UniqueId}.Script";

            return _script;
        }

        protected override IDatamodelMetadata GetDatamodel() => _datamodel;

        public DatamodelMetadata<RootStateMetadata> WithDatamodel()
        {
            _datamodel = new DatamodelMetadata<RootStateMetadata>();

            _datamodel.Parent = this;

            _datamodel.UniqueId = $"{((IModelMetadata)this).UniqueId}.Datamodel";

            return _datamodel;
        }

        public AtomicStateMetadata<RootStateMetadata> WithAtomicState(string id)
        {
            return WithState<AtomicStateMetadata<RootStateMetadata>>(id);
        }

        public SequentialStateMetadata<RootStateMetadata> WithSequentialState(string id)
        {
            return WithState<SequentialStateMetadata<RootStateMetadata>>(id);
        }

        public ParallelStateMetadata<RootStateMetadata> WithParallelState(string id)
        {
            return WithState<ParallelStateMetadata<RootStateMetadata>>(id);
        }

        public FinalStateMetadata<RootStateMetadata> WithFinalState(string id)
        {
            return WithState<FinalStateMetadata<RootStateMetadata>>(id);
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

        Databinding IRootStateMetadata.Databinding => _databinding;

        ITransitionMetadata IRootStateMetadata.GetInitialTransition()
        {
            if (_initialTransition != null)
            {
                return _initialTransition;
            }
            else if (_states.Count > 0)
            {
                return new TransitionMetadata<RootStateMetadata>()
                                    .WithTarget(((IStateMetadata)_states[0]).Id);
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
