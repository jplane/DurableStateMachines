using System.Collections.Generic;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using Nito.AsyncEx;
using System.Threading.Tasks;
using System;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class ParallelState : CompoundState
    {
        public ParallelState(IParallelStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
            metadata.CheckArgNull(nameof(metadata));

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
                    else if (stateMetadata is IHistoryStateMetadata hsm)
                    {
                        states.Add(new HistoryState(hsm, this));
                    }
                }

                return states.ToArray();
            });
        }

        public override bool IsParallelState => true;

        public override bool IsInFinalState(ExecutionContext context, RootState root)
        {
            foreach (var child in GetChildStates())
            {
                if (! child.IsInFinalState(context, root))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
