using StateChartsDotNet.Common;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Services
{
    internal static class Resolver
    {
        public static SendMessageMetadata Resolve(XElement element)
        {
            element.CheckArgNull(nameof(element));
            
            SendMessageMetadata metadata = element.Name.LocalName switch
            {
                "http-post" => new HttpPostMetadata(element),
                _ => throw new NotSupportedException("Unexpected service type: " + element.Name.LocalName),
            };

            return metadata;
        }
    }
}
