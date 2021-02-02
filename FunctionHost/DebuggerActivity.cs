using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSM.FunctionHost
{
    public static class StateMachineDebuggerActivity
    {
        public static async Task RunAsync(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger logger)
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
            //await signalr.SendAsync("register", context.InstanceId);

            try
            {
                await signalr.SendAsync("break", data.Config);
                //await signalr.SendAsync("break", context.InstanceId, data.Config);

                await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMinutes(2)));
            }
            finally
            {
                //await signalr.SendAsync("unregister", context.InstanceId);
                await signalr.StopAsync();
            }
        }
    }
}
