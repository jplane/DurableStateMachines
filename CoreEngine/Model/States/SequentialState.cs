using System;
using System.Collections.Generic;
using System.Linq;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class SequentialState : CompoundState
    {
        private readonly Lazy<Transition> _initialTransition;

        public SequentialState(ISequentialStateMetadata metadata, State parent)
            : base(metadata, parent)
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
                    else if (stateMetadata is IHistoryStateMetadata hsm)
                    {
                        states.Add(new HistoryState(hsm, this));
                    }
                }

                return states.ToArray();
            });
        }

        public override bool IsSequentialState => true;

        public override Transition GetInitialStateTransition()
        {
            return _initialTransition.Value;
        }

        public override bool IsInFinalState(ExecutionContext context, RootState root)
        {
            foreach (var child in GetChildStates())
            {
                if (child.IsInFinalState(context, root) && context.Configuration.Contains(child))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
