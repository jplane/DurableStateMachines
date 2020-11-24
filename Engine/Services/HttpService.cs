using Newtonsoft.Json;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Services
{
    internal static class HttpService
    {
        private static HttpClient _client = new HttpClient();

        public static async Task PostAsync(string url,
                                           string ignored,
                                           object content,
                                           IReadOnlyDictionary<string, object> parameters)
        {
            url.CheckArgNull(nameof(url));
            parameters.CheckArgNull(nameof(parameters));

            _client.DefaultRequestHeaders.Clear();

            var queryString = ResolveParameters(parameters);

            var uri = GetUri(url, queryString);

            Debug.Assert(uri != null);

            var response = await Invoke("POST", content, uri);

            Debug.Assert(response != null);

            response.EnsureSuccessStatusCode();
        }

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

        private static async Task<HttpResponseMessage> Invoke(string verb, object content, Uri uri)
        {
            switch (verb.ToUpperInvariant())
            {
                case "POST":

                    content.CheckArgNull(nameof(content));

                    var httpContent = new StringContent(JsonConvert.SerializeObject(content),
                                                        Encoding.UTF8,
                                                        "application/json");

                    return await _client.PostAsync(uri, httpContent);

                default:
                    throw new InvalidOperationException("HTTP verb not supported: " + verb);
            }
        }

        private static string ResolveParameters(IReadOnlyDictionary<string, object> parameters)
        {
            var builder = new StringBuilder();

            foreach (var param in parameters)
            {
                var value = JsonConvert.SerializeObject(param.Value);

                if (param.Key.StartsWith("?"))
                {
                    var key = new string(param.Key.Skip(1).ToArray());

                    builder.Append($"{key}={value}&");
                }
                else
                {
                    _client.DefaultRequestHeaders.Add(param.Key, value);
                }
            }

            return builder.ToString().Trim('&');
        }
    }
}
