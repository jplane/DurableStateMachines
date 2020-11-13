using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
{
    public class FinalStateMetadata : StateMetadata, IFinalStateMetadata
    {
        public FinalStateMetadata(XElement element)
            : base(element)
        {
        }

        public Task<IDonedataMetadata> GetDonedata()
        {
            var node = _element.ScxmlElement("donedata");

            return Task.FromResult(node == null ? null : (IDonedataMetadata) new DonedataMetadata(node));
        }
    }
}
