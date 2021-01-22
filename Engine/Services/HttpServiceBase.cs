using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
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
        private readonly dynamic _data;
        private readonly CancellationToken _token;

        protected HttpServiceBase(object data, CancellationToken token)
        {
            _data = data;
            _token = token;
        }

        public async Task<string> GetAsync(IQueryConfiguration input)
        {
            input.CheckArgNull(nameof(input));

            var config = (HttpQueryConfiguration) input;

            var headers = GetHeaders(config, null);

            var uri = GetUri(config);

            return await Invoke(headers, uri, null, null, HttpMethod.Get, _token);
        }

        public async Task PostAsync(string correlationId, ISendMessageConfiguration input)
        {
            input.CheckArgNull(nameof(input));

            var config = (HttpSendMessageConfiguration) input;

            var headers = GetHeaders(config, correlationId);

            var uri = GetUri(config);

            var serialized = Serialize(config);

            await Invoke(headers, uri, serialized.Content, serialized.ContentType, HttpMethod.Post, _token);
        }

        protected abstract Task<string> Invoke(IReadOnlyDictionary<string, string> headers,
                                               Uri uri,
                                               string content,
                                               string contentType,
                                               HttpMethod method,
                                               CancellationToken cancelToken);

        private Uri GetUri(HttpConfiguration config)
        {
            Debug.Assert(config != null);

            var queryString = GetQueryString(config);

            var baseUri = config.Uri;

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

        private (string Content, string ContentType) Serialize(HttpSendMessageConfiguration config)
        {
            Debug.Assert(config != null);

            var content = config.GetContent(_data);

            if (content is string s)
            {
                return (s, config.ContentType ?? "text/plain");
            }
            else
            {
                return (JsonConvert.SerializeObject(content), "application/json");
            }
        }

        private IReadOnlyDictionary<string, string> GetHeaders(HttpConfiguration config, string correlationId)
        {
            Debug.Assert(config != null);

            var headers = new Dictionary<string, string>();

            foreach (var param in config.Headers ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                headers.Add(param.Key, JsonConvert.SerializeObject(param.Value));
            }

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                headers.Add("X-SCDN-CORRELATION", correlationId);
            }

            return headers;
        }

        private string GetQueryString(HttpConfiguration config)
        {
            Debug.Assert(config != null);

            var builder = new StringBuilder();

            foreach (var param in config.QueryString ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                builder.Append($"{param.Key}={JsonConvert.SerializeObject(param.Value)}&");
            }

            return builder.ToString().Trim('&');
        }
    }
}
