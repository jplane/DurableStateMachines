using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Services
{
    internal abstract class HttpServiceBase
    {
        private readonly CancellationToken _token;

        protected HttpServiceBase(CancellationToken token)
        {
            _token = token;
        }

        public async Task<string> GetAsync(JObject config)
        {
            config.CheckArgNull(nameof(config));

            var headers = GetHeaders(config, null);

            var uri = GetUri(config);

            return await Invoke(headers.Headers, uri, null, null, HttpMethod.Get, _token);
        }

        public async Task PostAsync(string correlationId, JObject config)
        {
            config.CheckArgNull(nameof(config));

            var headers = GetHeaders(config, correlationId);

            var uri = GetUri(config);

            var content = config.Property("content")?.Value;

            var serialized = Serialize(content, headers.ContentType);

            await Invoke(headers.Headers, uri, serialized.Content, serialized.ContentType, HttpMethod.Post, _token);
        }

        protected abstract Task<string> Invoke(IReadOnlyDictionary<string, string> headers,
                                               Uri uri,
                                               string content,
                                               string contentType,
                                               HttpMethod method,
                                               CancellationToken cancelToken);

        private Uri GetUri(JObject config)
        {
            Debug.Assert(config != null);

            var queryString = GetQueryString(config);

            var baseUri = config.Property("uri")?.Value.Value<string>();

            if (string.IsNullOrWhiteSpace(baseUri))
            {
                throw new InvalidOperationException("Http operation requires configured 'uri' property.");
            }

            if (string.IsNullOrWhiteSpace(queryString))
            {
                return new Uri(baseUri);
            }
            else
            {
                return new Uri($"{baseUri}?{queryString}");
            }
        }

        private (string Content, string ContentType) Serialize(object content, string contentType)
        {
            if (content is string s)
            {
                return (s, contentType ?? "text/plain");
            }
            else
            {
                return (JsonConvert.SerializeObject(content), contentType ?? "application/json");
            }
        }

        private (IReadOnlyDictionary<string, string> Headers, string ContentType) GetHeaders(JObject config, string correlationId)
        {
            Debug.Assert(config != null);

            var parameters = config.Property("headers")?.Value.Value<Dictionary<string, object>>() ?? new Dictionary<string, object>();

            var headers = new Dictionary<string, string>();

            string contentType = null;

            foreach (var param in parameters)
            {
                if (string.Compare(param.Key, "content-type", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    contentType = (string) param.Value;
                }
                else
                {
                    headers.Add(param.Key, JsonConvert.SerializeObject(param.Value));
                }
            }

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                headers.Add("X-SCDN-CORRELATION", correlationId);
            }

            return (headers, contentType);
        }

        private string GetQueryString(JObject config)
        {
            Debug.Assert(config != null);

            var parameters = config.Property("querystring")?.Value.Value<Dictionary<string, object>>() ?? new Dictionary<string, object>();

            var builder = new StringBuilder();

            foreach (var param in parameters)
            {
                builder.Append($"{param.Key}={JsonConvert.SerializeObject(param.Value)}&");
            }

            return builder.ToString().Trim('&');
        }
    }
}
