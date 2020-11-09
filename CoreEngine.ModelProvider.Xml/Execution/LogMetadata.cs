﻿using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class LogMetadata : ExecutableContentMetadata, ILogMetadata
    {
        public LogMetadata(XElement element)
            : base(element)
        {
        }

        public string Message => _element.Attribute("expr")?.Value ?? string.Empty;
    }
}
