using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using DSM.FunctionClient;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            var config = (IConfiguration)host.Services.GetService(typeof(IConfiguration));
            Debug.Assert(config != null);

            await RunStateMachineFromName(host, config);

            await RunStateMachineFromDefinition(config);

            await host.RunAsync();
        }

        private static async Task RunStateMachineFromDefinition(IConfiguration config)
        {
            var json = JObject.Parse(File.ReadAllText("definition.json"));

            using var http = new HttpClient();

            var uri = $"{config["DF_ENDPOINT"]}/runtime/webhooks/durabletask/orchestrators/statemachine-definition";

            var response = await http.PostAsJsonAsync(uri, json);

            var outputJson = JObject.Parse(await response.Content.ReadAsStringAsync());

            uri = outputJson["statusQueryGetUri"].Value<string>();

            var done = false;

            while (!done)
            {
                await Task.Delay(1000);

                response = await http.GetAsync(uri);

                outputJson = JObject.Parse(await response.Content.ReadAsStringAsync());

                var status = outputJson["runtimeStatus"].Value<string>();

                done = status == "Failed" || status == "Canceled" || status == "Terminated" || status == "Completed";
            }

            Console.WriteLine(outputJson["runtimeStatus"].Value<string>());

            Console.WriteLine(outputJson["output"]);
        }

        private static async Task RunStateMachineFromName(IHost host, IConfiguration config)
        {
            var factory = (IDurableClientFactory) host.Services.GetService(typeof(IDurableClientFactory));

            Debug.Assert(factory != null);

            var client = factory.CreateClient(new DurableClientOptions
            {
                ConnectionName = "Storage",
                TaskHub = config["TaskHub"]
            });

            Debug.Assert(client != null);

            var instanceId = await client.StartNewStateMachineAsync("test1", (5, 0));

            var result = await client.WaitForStateMachineCompletionAsync(instanceId);

            Console.WriteLine(result.RuntimeStatus);

            Console.WriteLine(result.ToOutput<(int x, int y)>());
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .UseEnvironment("Development")
                    .ConfigureServices((_, services) => services.AddDurableClientFactory());
    }
}
