using StateChartsDotNet.CoreEngine.Abstractions.Model.States;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class AtomicState : State
    {
        public AtomicState(IAtomicStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override bool IsAtomic => true;
    }
}
