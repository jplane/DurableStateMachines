using CoreEngine.Abstractions.Model.Execution.Metadata;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class AssignMetadata : ExecutableContentMetadata, IAssignMetadata
    {
        public AssignMetadata(XElement element)
            : base(element)
        {
        }

        public string Location => _element.Attribute("location").Value;

        public string Expression => _element.Attribute("expr").Value ?? string.Empty;

        public string Body => _element.Value ?? string.Empty;
    }
}
