using CoreEngine.Abstractions.Model.States.Metadata;
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
