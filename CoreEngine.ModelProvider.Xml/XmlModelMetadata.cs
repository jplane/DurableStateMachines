using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.States;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml
{
    public class XmlModelMetadata : IModelMetadata
    {
        private readonly XDocument _document;

        public XmlModelMetadata(XDocument document)
        {
            _document = document;
        }

        public IRootStateMetadata GetRootState()
        {
            return new RootStateMetadata(_document.Root);
        }
    }
}
