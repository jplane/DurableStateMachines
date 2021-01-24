using ConsoleDebugger;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using StateChartsDotNet.DurableFunction.Client;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Delay(5000);

            using IHost host = CreateHostBuilder(args).Build();

            var machine = StateChart.Define("test")
                                    .DataInit("x", 1)
                                    .State("state1")
                                        .OnEntry
                                            .Assign("x", data => ((long)data["x"]) + 1)._
                                        .OnExit
                                            .Assign("x", data => ((long)data["x"]) + 1)._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var config = (IConfiguration) host.Services.GetService(typeof(IConfiguration));

            Debug.Assert(config != null);

            var hook = new DebugHook();

            hook.DebuggerUri = config["DEBUGGER_URI"];

            var debuggerTask = hook.StartAsync();

            using var client = new StateMachineHttpClient();

            client.BaseAddress = new Uri(config["DF_URI"]);

            await client.StartNewAsync(machine, hook.GetDebuggerInfo());

            DurableOrchestrationStatus status = null;

            var done = false;

            while (!done)
            {
                await Task.Delay(2000);

                status = await client.GetStatusAsync();

                done = status.RuntimeStatus == OrchestrationRuntimeStatus.Canceled ||
                       status.RuntimeStatus == OrchestrationRuntimeStatus.Completed ||
                       status.RuntimeStatus == OrchestrationRuntimeStatus.Failed ||
                       status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated;
            }

            if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var output = status.Output.ToObject<Dictionary<string, object>>();

                foreach (var key in output.Keys)
                {
                    Console.WriteLine($"{key} = {output[key]}");
                }
            }

            await host.RunAsync();

            await hook.StopAsync();
            
            await debuggerTask;
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseEnvironment("Development")
                    .ConfigureServices((_, services) => services.AddDurableClientFactory());
    }
}
