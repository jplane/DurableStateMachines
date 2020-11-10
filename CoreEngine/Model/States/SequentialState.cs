using System;
using System.Collections.Generic;
using System.Linq;
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.States.Metadata;
using System.Threading.Tasks;

namespace CoreEngine.Model.States
{
    internal class SequentialState : CompoundState
    {
        private readonly AsyncLazy<Transition> _initialTransition;

        public SequentialState(ISequentialStateMetadata metadata, State parent)
            : base(metadata, parent)
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
                    else if (stateMetadata is IHistoryStateMetadata hsm)
                    {
                        states.Add(new HistoryState(hsm, this));
                    }
                }

                return states.ToArray();
            });
        }

        public override bool IsSequentialState => true;

        public override Task<Transition> GetInitialStateTransition()
        {
            return _initialTransition.Task;
        }

        public override async Task<bool> IsInFinalState(ExecutionContext context, RootState root)
        {
            foreach (var child in await GetChildStates())
            {
                if (await child.IsInFinalState(context, root) && context.Configuration.Contains(child))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
