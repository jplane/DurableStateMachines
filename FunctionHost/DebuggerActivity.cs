using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSM.FunctionHost
{
    public class StateMachineDebuggerActivity
    {
        private readonly IConfiguration _config;
        private readonly ILogger<StateMachineDebuggerActivity> _logger;

        public StateMachineDebuggerActivity(IConfiguration config, ILogger<StateMachineDebuggerActivity> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task RunAsync(
            [ActivityTrigger] IDurableActivityContext context)
        {
            Debug.Assert(context != null);

            var data = context.GetInput<(string Uri, Dictionary<string, object> Config)>();

            Debug.Assert(!string.IsNullOrWhiteSpace(data.Uri));
            Debug.Assert(data.Config != null);

            var tcs = new TaskCompletionSource<bool>();

            var signalr = new HubConnectionBuilder()
                                .WithUrl(data.Uri)
                                .AddNewtonsoftJsonProtocol()
                                .Build();

            signalr.On("resume", () => tcs.SetResult(true));

            await signalr.StartAsync();
            await signalr.SendAsync("register", context.InstanceId);

            try
            {
                await signalr.SendAsync("break", context.InstanceId, data.Config);

                await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMinutes(2)));
            }
            finally
            {
                await signalr.SendAsync("unregister", context.InstanceId);
                await signalr.StopAsync();
            }
        }
    }
}
