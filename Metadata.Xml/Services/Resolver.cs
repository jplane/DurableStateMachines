using StateChartsDotNet.Common;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Services
{
    internal static class ServiceResolver
    {
        public static SendMessageMetadata Resolve(XElement element)
        {
            element.CheckArgNull(nameof(element));
            
            SendMessageMetadata metadata = element.Name.LocalName switch
            {
                "http-post" => new HttpPostMetadata(element),
                "send-parent" => new SendParentMetadata(element),
                "send-child" => new SendChildMetadata(element),
                _ => null
            };

            return metadata;
        }
    }
}
