using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Services
{
    internal static class HttpService
    {
        private static HttpClient _client = new HttpClient();

        public static async Task<string> GetAsync(string url,
                                                  IReadOnlyDictionary<string, object> parameters,
                                                  CancellationToken token)
        {
            url.CheckArgNull(nameof(url));
            parameters.CheckArgNull(nameof(parameters));

            _client.DefaultRequestHeaders.Clear();

            var queryString = ResolveParameters(parameters);

            var uri = GetUri(url, queryString);

            Debug.Assert(uri != null);

            var response = await Invoke("GET", null, uri, token);

            Debug.Assert(response != null);

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task PostAsync(string url,
                                           string _,
                                           object content,
                                           string correlationId,
                                           IReadOnlyDictionary<string, object> parameters,
                                           CancellationToken token)
        {
            url.CheckArgNull(nameof(url));
            parameters.CheckArgNull(nameof(parameters));

            _client.DefaultRequestHeaders.Clear();

            var queryString = ResolveParameters(parameters);

            AddCorrelationHeader(correlationId);

            var uri = GetUri(url, queryString);

            Debug.Assert(uri != null);

            var response = await Invoke("POST", content, uri, token);

            Debug.Assert(response != null);

            response.EnsureSuccessStatusCode();
        }

        public static async Task StartRemoteChildStatechartAsync(string remoteUri,
                                                                 IInvokeStateChartMetadata invokeMetadata,
                                                                 string metadataId,
                                                                 string instanceId,
                                                                 IDictionary<string, object> inputs,
                                                                 CancellationToken cancelToken)
        {
            remoteUri.CheckArgNull(nameof(remoteUri));
            invokeMetadata.CheckArgNull(nameof(invokeMetadata));
            metadataId.CheckArgNull(nameof(metadataId));
            instanceId.CheckArgNull(nameof(instanceId));
            inputs.CheckArgNull(nameof(inputs));

            inputs["_parentRemoteUri"] = $"{remoteUri}/api/sendmessage";

            var statechartMetadata = invokeMetadata.GetRoot();

            Debug.Assert(statechartMetadata != null);

            var content = new
            {
                statechart = statechartMetadata.ToJson(),
                metadataId = metadataId,
                instanceId = instanceId,
                inputs = inputs
            };

            await PostAsync($"{invokeMetadata.RemoteUri}/api/registerandstart",
                            null,
                            content,
                            null,
                            new Dictionary<string, object>(),
                            cancelToken);
        }

        private static void AddCorrelationHeader(string correlationId)
        {
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                _client.DefaultRequestHeaders.Add("X-SCDN-CORRELATION", correlationId);
            }
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

        private static async Task<HttpResponseMessage> Invoke(string verb,
                                                              object content,
                                                              Uri uri,
                                                              CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(verb));
            Debug.Assert(uri != null);

            switch (verb.ToUpperInvariant())
            {
                case "GET":
                    return await _client.GetAsync(uri, token);

                case "POST":

                    content.CheckArgNull(nameof(content));

                    var httpContent = new StringContent(JsonConvert.SerializeObject(content),
                                                        Encoding.UTF8,
                                                        "application/json");

                    return await _client.PostAsync(uri, httpContent, token);

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
