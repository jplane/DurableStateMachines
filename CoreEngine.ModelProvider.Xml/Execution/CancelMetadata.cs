using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class CancelMetadata : ExecutableContentMetadata, ICancelMetadata
    {
        public CancelMetadata(XElement element)
            : base(element)
        {
        }

        public string SendId => _element.Attribute("sendid")?.Value ?? string.Empty;

        public string SendIdExpr => _element.Attribute("sendidexpr")?.Value ?? string.Empty;
    }
}
