using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Primitives;
using DSM.Common;
using DSM.Engine.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DSM.FunctionHost
{
    internal class HttpService : HttpServiceBase
    {
        private readonly IDurableOrchestrationContext _context;

        public HttpService(object data, IDurableOrchestrationContext context)
            : base(data, default)
        {
            context.CheckArgNull(nameof(context));

            _context = context;
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
