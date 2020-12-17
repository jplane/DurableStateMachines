using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.States;

namespace StateChartsDotNet.Metadata.Json.States
{
    public class AtomicStateMetadata : StateMetadata, IAtomicStateMetadata
    {
        internal AtomicStateMetadata(JObject element)
            : base(element)
        {
        }
    }
}
