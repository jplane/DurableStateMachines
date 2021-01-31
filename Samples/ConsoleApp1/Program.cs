using ClassLibrary1;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using StateChartsDotNet.DurableFunctionClient;
using System;
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

            var config = (IConfiguration) host.Services.GetService(typeof(IConfiguration));
            Debug.Assert(config != null);

            var factory = (IDurableClientFactory) host.Services.GetService(typeof(IDurableClientFactory));
            Debug.Assert(factory != null);

            var client = factory.CreateClient(new DurableClientOptions
            {
                ConnectionName = "Storage",
                TaskHub = config["TaskHub"]
            });

            Debug.Assert(client != null);

            //var hook = new DebugHook();

            //hook.DebuggerUri = config["DEBUGGER_URI"];

            //var debuggerTask = hook.StartAsync();

            var state = new TestState
            {
                X = 5
            };

            var instanceId = await client.StartNewStateMachineAsync("test", state);

            var output = await client.WaitForStateMachineCompletionAsync<TestState>(instanceId);

            Console.WriteLine(output.Item2.RuntimeStatus);

            Console.WriteLine(output.Item1.X);

            await host.RunAsync();

            //await hook.StopAsync();
            
            //await debuggerTask;
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseEnvironment("Development")
                    .ConfigureServices((_, services) => services.AddDurableClientFactory());
    }
}
