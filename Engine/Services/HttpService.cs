using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Services
{
    internal class HttpService : HttpServiceBase
    {
        private static HttpClient _client = new HttpClient();

        public HttpService(object data, CancellationToken token)
            : base(data, token)
        {
        }

        protected override async Task<string> Invoke(IReadOnlyDictionary<string, string> headers,
                                                     Uri uri,
                                                     string content,
                                                     string contentType,
                                                     HttpMethod method,
                                                     CancellationToken cancelToken)
        {
            Debug.Assert(headers != null);
            Debug.Assert(uri != null);

            var msg = new HttpRequestMessage();

            msg.RequestUri = uri;

            foreach (var item in headers)
            {
                msg.Headers.Add(item.Key, item.Value);
            }

            msg.Method = method;

            if (! string.IsNullOrWhiteSpace(content))
            {
                msg.Content = new StringContent(content, Encoding.UTF8, contentType);
            }

            var response = await _client.SendAsync(msg, cancelToken);

            Debug.Assert(response != null);

            var result = string.Empty;

            if (method == HttpMethod.Get)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}
