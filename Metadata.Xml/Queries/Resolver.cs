using StateChartsDotNet.Common;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Queries
{
    internal static class QueryResolver
    {
        public static QueryMetadata Resolve(XElement element)
        {
            element.CheckArgNull(nameof(element));
            
            QueryMetadata metadata = element.Name.LocalName switch
            {
                "http-get" => new HttpGetMetadata(element),
                _ => null
            };

            return metadata;
        }
    }
}
