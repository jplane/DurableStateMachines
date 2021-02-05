using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace DSM.FunctionHost
{
    public static class ObserverActivity
    {
        private static readonly ConcurrentDictionary<string, AsyncLazy<HubConnection>> _signalrConns =
            new ConcurrentDictionary<string, AsyncLazy<HubConnection>>();

        internal static async Task StopConnectionAsync(string instanceId)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            if (_signalrConns.TryRemove(instanceId, out AsyncLazy<HubConnection> conn))
            {
                await (await conn).SendAsync("unregister", instanceId);
                await (await conn).StopAsync();
            }
        }

        public static async Task RunAsync(
            [ActivityTrigger] IDurableActivityContext context,
            ILogger logger)
        {
            Debug.Assert(context != null);

            var data = context.GetInput<(string Uri, Dictionary<string, object> Config, string[] InstanceStack)>();

            Debug.Assert(!string.IsNullOrWhiteSpace(data.Uri));
            Debug.Assert(data.Config != null);

            var observabilityInstanceId = data.InstanceStack?.LastOrDefault() ?? context.InstanceId;

            Debug.Assert(!string.IsNullOrWhiteSpace(observabilityInstanceId));

            var conn = await _signalrConns.GetOrAdd(observabilityInstanceId, _ =>
                new AsyncLazy<HubConnection>(async () =>
                {
                    var signalr = new HubConnectionBuilder()
                            .WithUrl(data.Uri)
                            .AddNewtonsoftJsonProtocol()
                            .Build();

                    await signalr.StartAsync();
                    await signalr.SendAsync("register", observabilityInstanceId);

                    return signalr;
                }));

            var tcs = new TaskCompletionSource<bool>();

            using var disposable = conn.On("resume", () => tcs.SetResult(true));

            await conn.SendAsync("break", observabilityInstanceId, data.Config);

            await tcs.Task;
        }
    }
}
