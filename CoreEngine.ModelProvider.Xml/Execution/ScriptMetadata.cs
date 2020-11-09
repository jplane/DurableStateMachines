using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class ScriptMetadata : ExecutableContentMetadata, IScriptMetadata
    {
        public ScriptMetadata(XElement element)
            : base(element)
        {
        }

        public string Source => _element.Attribute("src")?.Value ?? string.Empty;

        public string BodyExpression => _element.Value ?? string.Empty;
    }
}
