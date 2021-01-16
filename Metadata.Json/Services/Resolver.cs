using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Text;


namespace StateChartsDotNet.Metadata.Json.Services
{
    internal static class ServiceResolver
    {
        public static SendMessageMetadata Resolve(JObject element)
        {
            element.CheckArgNull(nameof(element));

            var type = element.Property("type").Value.Value<string>();

            SendMessageMetadata metadata = type switch
            {
                "http-post" => new HttpPostMetadata(element),
                _ => null
            };

            return metadata;
        }
    }
}
