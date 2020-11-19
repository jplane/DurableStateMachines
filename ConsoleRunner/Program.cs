using StateChartsDotNet.CoreEngine;
using System;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml;
using StateChartsDotNet.CoreEngine.Abstractions;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleRunner
{
    public class Foo
    {
        public dynamic data;
    }

    public class ScxmlTestEventObject
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ScxmlTestEvent
    {
        [JsonPropertyName("event")]
        public ScxmlTestEventObject Event { get; set; }

        [JsonPropertyName("nextConfiguration")]
        public List<string> NextConfiguration { get; set; }
    }

    public class ScxmlTest
    {
        [JsonPropertyName("initialConfiguration")]
        public List<string> InitialConfiguration { get; set; }

        [JsonPropertyName("events")]
        public List<ScxmlTestEvent> Events { get; set; }
    }

    class Program
    {
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

                // task = RunForeach(logger);

                task = RunScxmlTests(logger);
            }

            Task.WaitAll(Task.Delay(5000), task);
        }

        static Task RunForeach(ILogger logger)
        {
            return Run("foreach.xml", logger);
        }

        static Task RunMicrowave(ILogger logger)
        {
            return Run("microwave.xml", logger, async queue =>
            {
                queue.Enqueue(new Message("turn.on"));
                await Task.Delay(500);

                for (var i = 0; i < 5; i++)
                {
                    queue.Enqueue(new Message("time"));
                    await Task.Delay(500);
                }

                queue.Enqueue(new Message("cancel"));
                await Task.Delay(500);
            });
        }

        static Task RunScxmlTests(ILogger logger)
        {
            var jsonPath = "../test-framework/test/basic/basic1.json";
            var testJson = System.IO.File.ReadAllText(jsonPath);

            var scxmlTest = JsonSerializer.Deserialize<ScxmlTest>(testJson);
            return Run("../test-framework/test/basic/basic1.scxml", logger, async queue =>
            {
                foreach (var item in scxmlTest.Events)
                {
                    Console.WriteLine(item.Event.Name);
                    queue.Enqueue(new Message(item.Event.Name));


                    // TODO: verify that the interpreter's state configuration
                    // contains item.nextConfiguration (sets are equal)
                }
            });
        }

        static Task Run(string xmldoc, ILogger logger, Func<Queue<Message>, Task> action = null)
        {
            var metadata = new XmlModelMetadata(XDocument.Load(xmldoc));

            var queue = new Queue<Message>();

            var runTask = Interpreter.Run(metadata, queue, logger);

            var actionTask = Task.CompletedTask;

            if (action != null)
            {
                actionTask = action.Invoke(queue);
            }

            return Task.WhenAll(runTask, actionTask);
        }
    }
}
