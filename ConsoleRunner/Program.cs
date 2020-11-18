using StateChartsDotNet.CoreEngine;
using System;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml;
using StateChartsDotNet.CoreEngine.Abstractions;
using System.Collections.Generic;

namespace ConsoleRunner
{
    public class Foo
    {
        public dynamic data;
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
            return Run("microwave.xml", logger, async context =>
            {
                await context.SendAsync("turn.on");

                for (var i = 0; i < 5; i++)
                {
                    await context.SendAsync("time");
                }

                await context.SendAsync("cancel");
            });
        }

        static Task Run(string xmldoc, ILogger logger, Func<ExecutionContext, Task> action = null)
        {
            var metadata = new XmlModelMetadata(XDocument.Load(xmldoc));

            var context = new ExecutionContext();

            context.Logger = logger;

            var interpreter = new Interpreter();

            var runTask = interpreter.Run(metadata, context);

            Task externalTask = Task.CompletedTask;

            if (action != null)
            {
                externalTask = action.Invoke(context);
            }

            return Task.WhenAll(runTask, externalTask);
        }
    }
}
