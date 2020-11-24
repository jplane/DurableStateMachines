using StateChartsDotNet.Common;
using StateChartsDotNet.Metadata.Xml.Execution;
using StateChartsDotNet.Services.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace StateChartsDotNet.Services
{
    public static class XmlResolver
    {
        public static SendMessageMetadata Resolve(XElement element)
        {
            element.CheckArgNull(nameof(element));
            
            SendMessageMetadata metadata = element.Name.LocalName switch
            {
                "http-post" => new XmlMetadata(element),
                _ => throw new NotSupportedException("Unexpected service type: " + element.Name.LocalName),
            };

            return metadata;
        }
    }
}
