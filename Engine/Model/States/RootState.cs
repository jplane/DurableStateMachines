using System;
using System.Collections.Generic;
using StateChartsDotNet.Model.Execution;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Model;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.States
{
    internal class RootState : CompoundState
    {
        private readonly Lazy<Script> _script;
        private readonly Lazy<Transition> _initialTransition;
        
        public RootState(IRootStateMetadata metadata)
            : base(metadata, null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _initialTransition = new Lazy<Transition>(() =>
            {
                var meta = metadata.GetInitialTransition();

                if (meta != null)
                    return new Transition(meta, this);
                else
                    return null;
            });

            _script = new Lazy<Script>(() =>
            {
                var meta = metadata.GetScript();

                if (meta != null)
                    return new Script(meta);
                else
                    return null;
            });

            _states = new Lazy<State[]>(() =>
            {
                var states = new List<State>();

                foreach (var stateMetadata in metadata.GetStates())
                {
                    if (stateMetadata is ISequentialStateMetadata ssm)
                    {
                        states.Add(new SequentialState(ssm, this));
                    }
                    else if (stateMetadata is IParallelStateMetadata psm)
                    {
                        states.Add(new ParallelState(psm, this));
                    }
                    else if (stateMetadata is IAtomicStateMetadata asm)
                    {
                        states.Add(new AtomicState(asm, this));
                    }
                    else if (stateMetadata is IFinalStateMetadata fsm)
                    {
                        states.Add(new FinalState(fsm, this));
                    }
                }

                return states.ToArray();
            });
        }

        public void ExecuteScript(ExecutionContext context)
        {
            _script.Value?.ExecuteAsync(context);
        }

        public Databinding Binding => ((IRootStateMetadata) _metadata).Databinding;

        public string Name => ((IRootStateMetadata) _metadata).Id;

        public override string Id => "scxml_root";

        public override bool IsScxmlRoot => true;

        public override Task InvokeAsync(ExecutionContext context)
        {
            throw new InvalidOperationException("Unexpected invocation.");
        }

        public override Transition GetInitialStateTransition()
        {
            return _initialTransition.Value;
        }

        public override void RecordHistory(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
