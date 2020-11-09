using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class RaiseMetadata : ExecutableContentMetadata, IRaiseMetadata
    {
        public RaiseMetadata(XElement element)
            : base(element)
        {
        }

        public string Event => _element.Attribute("event").Value;
    }
}
