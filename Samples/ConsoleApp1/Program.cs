using ClassLibrary1;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DSM.FunctionClient;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
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

            await using var observer = new DebugHook
            {
                EndpointUri = config["DEBUGGER_URI"]
            };

            var instanceId = await client.StartNewStateMachineAsync("tupletest", (5, 0), observer);

            var result = await client.WaitForStateMachineCompletionAsync(instanceId);

            Console.WriteLine(result.RuntimeStatus);

            Console.WriteLine(result.ToOutput<(int x, int y)>());

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseEnvironment("Development")
                    .ConfigureServices((_, services) => services.AddDurableClientFactory());
    }
}
