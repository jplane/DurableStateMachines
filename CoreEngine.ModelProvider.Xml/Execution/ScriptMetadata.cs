using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class ScriptMetadata : ExecutableContentMetadata, IScriptMetadata
    {
        public ScriptMetadata(XElement element)
            : base(element)
        {
        }

        private string Source => _element.Attribute("src")?.Value ?? string.Empty;

        private string BodyExpression => _element.Value ?? string.Empty;

        public Task Execute(dynamic data)
        {
            throw new NotImplementedException();
        }
    }
}
