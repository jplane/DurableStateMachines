using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public static async Task PutAsync(string url,
                                          string _,
                                          object content,
                                          string correlationId,
                                          IReadOnlyDictionary<string, object> parameters,
                                          CancellationToken token)
        {
            url.CheckArgNull(nameof(url));
            parameters.CheckArgNull(nameof(parameters));

            var response = await Invoke("PUT", content, url, correlationId, parameters, token);

            Debug.Assert(response != null);

            response.EnsureSuccessStatusCode();
        }

        public static async Task StartRemoteChildStatechartAsync(string callbackUri,
                                                                 IInvokeStateChartMetadata invokeMetadata,
                                                                 string metadataId,
                                                                 string instanceId,
                                                                 IDictionary<string, object> inputs,
                                                                 CancellationToken cancelToken)
        {
            callbackUri.CheckArgNull(nameof(callbackUri));
            invokeMetadata.CheckArgNull(nameof(invokeMetadata));
            metadataId.CheckArgNull(nameof(metadataId));
            instanceId.CheckArgNull(nameof(instanceId));
            inputs.CheckArgNull(nameof(inputs));

            inputs["_parentRemoteUri"] = $"{callbackUri}/api/sendmessage/";

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

            foreach (var input in inputs)
            {
                parameters.Add($"X-SCDN-PARAM-{input.Key}", input.Value);
            }

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

            var values = ResolveParameters(parameters);

            var uri = GetUri(url, values.Item1);

            foreach (var item in values.Item2)
            {
                msg.Headers.Add(item.Key, item.Value);
            }

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
                    msg.Content = Serialize(content, values.Item3);
                    break;

                case "PUT":
                    msg.Method = HttpMethod.Put;

                    if (content != null)
                    {
                        msg.Content = Serialize(content, values.Item3);
                    }

                    break;

                default:
                    throw new InvalidOperationException("HTTP verb not supported: " + verb);
            }

            return await _client.SendAsync(msg, token);
        }

        private static StringContent Serialize(object content, string contentType)
        {
            if (content is string s)
            {
                return new StringContent(s, Encoding.UTF8, contentType ?? "text/plain");
            }
            else
            {
                return new StringContent(JsonConvert.SerializeObject(content),
                                         Encoding.UTF8,
                                         contentType ?? "application/json");
            }
        }

        private static (string, IReadOnlyDictionary<string, string>, string) ResolveParameters(IReadOnlyDictionary<string, object> parameters)
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

            return (builder.ToString().Trim('&'), headers, contentType);
        }
    }
}
