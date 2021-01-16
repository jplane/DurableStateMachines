using Newtonsoft.Json;
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

        public async Task<string> GetAsync(string url,
                                           IReadOnlyDictionary<string, object> parameters)
        {
            url.CheckArgNull(nameof(url));
            parameters.CheckArgNull(nameof(parameters));

            var parms = ResolveParameters(parameters, null);

            var uri = GetUri(url, parms.QueryString);

            return await Invoke(parms.Headers, uri, null, null, HttpMethod.Get, _token);
        }

        public async Task PostAsync(string url,
                                    string _,
                                    object content,
                                    string correlationId,
                                    IReadOnlyDictionary<string, object> parameters)
        {
            url.CheckArgNull(nameof(url));
            parameters.CheckArgNull(nameof(parameters));

            var parms = ResolveParameters(parameters, correlationId);

            var uri = GetUri(url, parms.QueryString);

            var serialized = Serialize(content, parms.ContentType);

            await Invoke(parms.Headers, uri, serialized.Content, serialized.ContentType, HttpMethod.Post, _token);
        }

        public async Task PutAsync(string url,
                                   string _,
                                   object content,
                                   string correlationId,
                                   IReadOnlyDictionary<string, object> parameters)
        {
            url.CheckArgNull(nameof(url));
            parameters.CheckArgNull(nameof(parameters));

            var parms = ResolveParameters(parameters, correlationId);

            string serializedContent = null;
            var contentType = parms.ContentType;

            var uri = GetUri(url, parms.QueryString);

            if (content != null)
            {
                var serialized = Serialize(content, parms.ContentType);

                serializedContent = serialized.Content;
                contentType = serialized.ContentType;
            }

            await Invoke(parms.Headers, uri, serializedContent, contentType, HttpMethod.Put, _token);
        }

        protected abstract Task<string> Invoke(IReadOnlyDictionary<string, string> headers,
                                               Uri uri,
                                               string content,
                                               string contentType,
                                               HttpMethod method,
                                               CancellationToken cancelToken);

        private static Uri GetUri(string url, string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return new Uri(url);
            }
            else
            {
                return new Uri($"{url}?{queryString}");
            }
        }

        private static (string Content, string ContentType) Serialize(object content, string contentType)
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

        private static (string QueryString, IReadOnlyDictionary<string, string> Headers, string ContentType)
            ResolveParameters(IReadOnlyDictionary<string, object> parameters, string correlationId)
        {
            Debug.Assert(parameters != null);

            var builder = new StringBuilder();

            var headers = new Dictionary<string, string>();

            string contentType = null;

            foreach (var param in parameters)
            {
                var value = JsonConvert.SerializeObject(param.Value);

                if (param.Key.StartsWith("?"))
                {
                    var key = new string(param.Key.Skip(1).ToArray());

                    builder.Append($"{key}={value}&");
                }
                else if (string.Compare(param.Key, "content-type", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
                {
                    contentType = (string) param.Value;
                }
                else
                {
                    headers.Add(param.Key, value);
                }
            }

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                headers.Add("X-SCDN-CORRELATION", correlationId);
            }

            return (builder.ToString().Trim('&'), headers, contentType);
        }
    }
}
