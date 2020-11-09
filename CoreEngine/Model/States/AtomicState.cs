using CoreEngine.Abstractions.Model.States.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

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
