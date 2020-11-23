using StateChartsDotNet.Common.Model.States;

namespace StateChartsDotNet.Model.States
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
