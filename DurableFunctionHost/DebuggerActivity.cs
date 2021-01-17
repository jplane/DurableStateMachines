using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace StateChartsDotNet.DurableFunctionHost
{
    public class DebuggerActivity
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DebuggerActivity> _logger;

        public DebuggerActivity(IConfiguration config, ILogger<DebuggerActivity> logger)
        {
            _config = config;
            _logger = logger;
        }

        [FunctionName("debugger-break")]
        public async Task RunAsync(
            [ActivityTrigger] IDurableActivityContext context)
        //            [DurableClient] IDurableClient client)
        {
            Debug.Assert(context != null);
            //Debug.Assert(client != null);

            var data = context.GetInput<(string Uri, Dictionary<string, object> Config)>();

            Debug.Assert(!string.IsNullOrWhiteSpace(data.Uri));
            Debug.Assert(data.Config != null);

            //var mgmtInfo = client.CreateHttpManagementPayload(context.InstanceId);

            //Debug.Assert(mgmtInfo != null);

            var tcs = new TaskCompletionSource<bool>();

            var signalr = new HubConnectionBuilder().WithUrl(data.Uri).Build();

            signalr.On("resume", () => tcs.SetResult(true));

            await signalr.StartAsync();

            try
            {
                await signalr.SendAsync("break", /* mgmtInfo.SendEventPostUri, */ data.Config);

                await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMinutes(2)));
            }
            finally
            {
                await signalr.StopAsync();
            }
        }
    }
}
