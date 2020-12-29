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
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
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

            var response = await Invoke("GET", null, url, null, parameters, token);

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

            var response = await Invoke("POST", content, url, correlationId, parameters, token);

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

            var tuple = await statechartMetadata.ToStringAsync(cancelToken);

            var parameters = new Dictionary<string, object>();

            switch (tuple.Item1)
            {
                case "fluent":
                    parameters.Add("Content-Type", "text/plain");
                    break;

                case "json":
                    parameters.Add("Content-Type", "application/json");
                    break;

                case "xml":
                    parameters.Add("Content-Type", "application/xml");
                    break;
            }

            parameters.Add("X-SCDN-PARAMS", JsonConvert.SerializeObject(inputs));
            parameters.Add("X-SCDN-METADATA-ID", metadataId);
            parameters.Add("X-SCDN-INSTANCE-ID", instanceId);

            await PostAsync($"{invokeMetadata.RemoteUri}/api/registerandstart",
                            null,
                            tuple.Item2,
                            null,
                            parameters,
                            cancelToken);
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
                                                              string url,
                                                              string correlationId,
                                                              IReadOnlyDictionary<string, object> parameters,
                                                              CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(verb));
            Debug.Assert(!string.IsNullOrWhiteSpace(url));
            Debug.Assert(parameters != null);

            var msg = new HttpRequestMessage();

            var queryString = ResolveParameters(parameters, msg.Headers);

            var uri = GetUri(url, queryString);

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                msg.Headers.Add("X-SCDN-CORRELATION", correlationId);
            }

            msg.RequestUri = uri;

            switch (verb.ToUpperInvariant())
            {
                case "GET":
                    msg.Method = HttpMethod.Get;
                    break;

                case "POST":
                    content.CheckArgNull(nameof(content));
                    msg.Method = HttpMethod.Post;
                    msg.Content = Serialize(content);
                    break;

                case "PUT":
                    msg.Method = HttpMethod.Put;

                    if (content != null)
                    {
                        msg.Content = Serialize(content);
                    }

                    break;

                default:
                    throw new InvalidOperationException("HTTP verb not supported: " + verb);
            }

            return await _client.SendAsync(msg, token);
        }

        private static StringContent Serialize(object content)
        {
            if (content is string s)
            {
                return new StringContent(s);
            }
            else
            {
                return new StringContent(JsonConvert.SerializeObject(content),
                                         Encoding.UTF8,
                                         "application/json");
            }
        }

        private static string ResolveParameters(IReadOnlyDictionary<string, object> parameters,
                                                HttpRequestHeaders headers)
        {
            Debug.Assert(parameters != null);
            Debug.Assert(headers != null);

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
                    headers.Add(param.Key, value);
                }
            }

            return builder.ToString().Trim('&');
        }
    }
}
