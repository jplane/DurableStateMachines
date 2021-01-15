﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using StateChartsDotNet.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunctionHost
{
    internal static class HttpActivities
    {
        [FunctionName("http-get")]
        public static Task<string> HttpGet([ActivityTrigger] IDurableActivityContext context)
        {
            Debug.Assert(context != null);

            var tuple = context.GetInput<(string, Dictionary<string, object>)>();

            return HttpService.GetAsync(tuple.Item1, tuple.Item2, default);
        }

        [FunctionName("http-post")]
        public static Task HttpPost([ActivityTrigger] IDurableActivityContext context)
        {
            Debug.Assert(context != null);

            var tuple = context.GetInput<(string, string, object, string, Dictionary<string, object>)>();

            return HttpService.PostAsync(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, default);
        }

        [FunctionName("http-put")]
        public static Task HttpPut([ActivityTrigger] IDurableActivityContext context)
        {
            Debug.Assert(context != null);

            var tuple = context.GetInput<(string, string, object, string, Dictionary<string, object>)>();

            return HttpService.PutAsync(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, default);
        }
    }
}
