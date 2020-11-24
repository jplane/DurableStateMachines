using StateChartsDotNet.Common.Model.States;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class AtomicStateMetadata : StateMetadata, IAtomicStateMetadata
    {
        internal AtomicStateMetadata(XElement element)
            : base(element)
        {
        }
    }
}
