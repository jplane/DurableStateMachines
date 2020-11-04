using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.States
{
    internal class AtomicState : State
    {
        public AtomicState(XElement element, State parent)
            : base(element, parent)
        {
        }

        public override bool IsAtomic => true;
    }
}
