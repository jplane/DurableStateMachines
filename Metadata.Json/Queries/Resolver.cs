using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Text;


namespace StateChartsDotNet.Metadata.Json.Queries
{
    internal static class QueryResolver
    {
        public static QueryMetadata Resolve(JObject element)
        {
            element.CheckArgNull(nameof(element));

            var type = element.Property("type").Value.Value<string>();

            QueryMetadata metadata = type switch
            {
                "http-get" => new HttpGetMetadata(element),
                _ => null
            };

            return metadata;
        }
    }
}
