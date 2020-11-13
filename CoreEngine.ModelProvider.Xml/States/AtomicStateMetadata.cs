using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
{
    public class AtomicStateMetadata : StateMetadata, IAtomicStateMetadata
    {
        public AtomicStateMetadata(XElement element)
            : base(element)
        {
        }
    }
}
