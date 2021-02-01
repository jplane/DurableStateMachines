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

            var instanceId = await client.StartNewStateMachineAsync("tupletest", (5, 0));

            var output = await client.WaitForStateMachineCompletionAsync<(int x, int y)>(instanceId);

            Console.WriteLine(output.Item2.RuntimeStatus);

            Console.WriteLine(output.Item1.x);

            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseEnvironment("Development")
                    .ConfigureServices((_, services) => services.AddDurableClientFactory());
    }
}
