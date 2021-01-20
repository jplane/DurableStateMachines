using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateChartsDotNet.Services
{
    public abstract class HttpConfiguration
    {
        public virtual void ResolveConfigValues(Func<string, string> resolver)
        {
            resolver.CheckArgNull(nameof(resolver));

            this.Uri = resolver(this.Uri);

            this.Headers = this.Headers?.ToDictionary(pair => pair.Key, pair => resolver(pair.Value));

            this.QueryString = this.QueryString?.ToDictionary(pair => pair.Key, pair => resolver(pair.Value));
        }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("headers")]
        public IReadOnlyDictionary<string, string> Headers { get; set; }

        [JsonProperty("querystring")]
        public IReadOnlyDictionary<string, string> QueryString { get; set; }
    }

    public class HttpQueryConfiguration : HttpConfiguration, IQueryConfiguration
    {
    }

    public class HttpSendMessageConfiguration : HttpConfiguration, ISendMessageConfiguration
    {
        [JsonProperty("contenttype")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public object Content { get; set; }
    }
}
