using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;
using CoreEngine.Abstractions.Model.States.Metadata;
using Nito.AsyncEx;
using System.Threading.Tasks;

namespace CoreEngine.Model.States
{
    internal class ParallelState : CompoundState
    {
        public ParallelState(IParallelStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
            metadata.CheckArgNull(nameof(metadata));

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
                    else if (stateMetadata is IHistoryStateMetadata hsm)
                    {
                        states.Add(new HistoryState(hsm, this));
                    }
                }

                return states.ToArray();
            });
        }

        public override bool IsParallelState => true;

        public override async Task<bool> IsInFinalState(ExecutionContext context, RootState root)
        {
            foreach (var child in await GetChildStates())
            {
                if (! await child.IsInFinalState(context, root))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
