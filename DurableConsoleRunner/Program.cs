using StateChartsDotNet.CoreEngine;
using System;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml;
using StateChartsDotNet.CoreEngine.Abstractions;
using System.Collections.Generic;
using StateChartsDotNet.CoreEngine.DurableTask;
using DurableTask.AzureStorage;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.States;

namespace DurableConsoleRunner
{
    class Program
    {
        const string AppName = "";
        const string TaskHubName = "";
        const string StorageConnectionString = "";

        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(
                    builder => builder.AddFilter("Default", LogLevel.Information)
                                      .AddConsole());

            var logger = loggerFactory.CreateLogger("StateChart");

            Task task;

            using (var scope = logger.BeginScope(""))
            {
                //task = RunMicrowave(logger);

                task = RunForeach(logger);
            }

            Task.WaitAll(Task.Delay(5000), task);
        }

        static Task RunForeach(ILogger logger)
        {
            return Run("foreach.xml", logger);
        }

        static Task RunMicrowave(ILogger logger)
        {
            return Run("microwave.xml", logger, async client =>
            {
                await client.SendMessageAsync(new Message("turn.on"));

                for (var i = 0; i < 5; i++)
                {
                    await client.SendMessageAsync(new Message("time"));
                }

                await client.SendMessageAsync(new Message("cancel"));
            });
        }

        static async Task Run(string xmldoc, ILogger logger, Func<DurableStateChartClient, Task> action = null)
        {
            var metadata = new RootStateMetadata(XDocument.Load(xmldoc));

            var settings = new AzureStorageOrchestrationServiceSettings
            {
                AppName = AppName,
                TaskHubName = TaskHubName,
                StorageConnectionString = StorageConnectionString
            };

            var dtazure = new AzureStorageOrchestrationService(settings);

            await dtazure.CreateIfNotExistsAsync();

            var service = new DurableStateChartService(metadata, dtazure, logger);

            await service.StartAsync();

            var client = new DurableStateChartClient(dtazure);

            await client.StartAsync();

            var actionTask = Task.CompletedTask;

            if (action != null)
            {
                actionTask = action.Invoke(client);
            }

            var completedTask = client.WaitForCompletionAsync(TimeSpan.MaxValue);

            await Task.WhenAll(actionTask, completedTask);

            await service.StopAsync();
        }
    }
}
