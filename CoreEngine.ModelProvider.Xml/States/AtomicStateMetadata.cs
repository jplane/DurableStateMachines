using CoreEngine.Abstractions.Model.States.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public class AtomicStateMetadata : StateMetadata, IAtomicStateMetadata
    {
        public AtomicStateMetadata(XElement element)
            : base(element)
        {
        }
    }
}
