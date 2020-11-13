using System;
using System.Collections.Generic;
using StateChartsDotNet.CoreEngine.Model.Execution;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class RootState : CompoundState
    {
        private readonly AsyncLazy<Script> _script;
        private readonly AsyncLazy<Transition> _initialTransition;
        
        public RootState(IRootStateMetadata metadata)
            : base(metadata, null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _initialTransition = new AsyncLazy<Transition>(async () =>
            {
                var meta = await metadata.GetInitialTransition();

                if (meta != null)
                    return new Transition(meta, this);
                else
                    return null;
            });

            _script = new AsyncLazy<Script>(async () =>
            {
                var meta = await metadata.GetScript();

                if (meta != null)
                    return new Script(meta);
                else
                    return null;
            });

            _states = new AsyncLazy<State[]>(async () =>
            {
                var states = new List<State>();

                foreach (var stateMetadata in await metadata.GetStates())
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

        public async Task ExecuteScript(ExecutionContext context)
        {
            var script = await _script;

            if (script != null)
            {
                await script.Execute(context);
            }
        }

        public Databinding Binding => ((IRootStateMetadata) _metadata).Databinding;

        public string Name => ((IRootStateMetadata) _metadata).Id;

        public override string Id => "scxml_root";

        public override bool IsScxmlRoot => true;

        public override Task Invoke(ExecutionContext context, RootState root)
        {
            throw new InvalidOperationException("Unexpected invocation.");
        }

        public override Task<Transition> GetInitialStateTransition()
        {
            return _initialTransition.Task;
        }

        public override Task RecordHistory(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
