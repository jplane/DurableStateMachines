using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Primitives;
using StateChartsDotNet.Common;
using StateChartsDotNet.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunctionHost
{
    internal class HttpService : HttpServiceBase
    {
        private readonly IDurableOrchestrationContext _context;
        private readonly Func<string, string> _resolveConfigValue;

        public HttpService(IDurableOrchestrationContext context, Func<string, string> resolveConfigValue)
            : base(default)
        {
            context.CheckArgNull(nameof(context));
            resolveConfigValue.CheckArgNull(nameof(resolveConfigValue));

            _context = context;
            _resolveConfigValue = resolveConfigValue;
        }

        protected override string ResolveConfigValue(string value)
        {
            return _resolveConfigValue(value);
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

            var convertedHeaders = headers.ToDictionary(pair => pair.Key,
                                                        pair => new StringValues(pair.Value));

            convertedHeaders.Add("Content-Type", contentType);

            var msg = new DurableHttpRequest(method, uri, convertedHeaders, content);

            var response = await _context.CallHttpAsync(msg);

            Debug.Assert(response != null);

            return response.Content;
        }
    }
}
