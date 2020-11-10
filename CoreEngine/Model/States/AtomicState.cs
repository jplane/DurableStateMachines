using CoreEngine.Abstractions.Model.States.Metadata;

namespace CoreEngine.Model.States
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
