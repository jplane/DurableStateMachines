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
                task = RunMicrowave(logger);

                //task = RunForeach(logger);
            }

            Task.WaitAll(Task.Delay(5000), task);
        }

        static Task RunForeach(ILogger logger)
        {
            return Run("foreach.xml", logger);
        }

        static Task RunMicrowave(ILogger logger)
        {
            return Run("microwave.xml", logger, context =>
            {
                context.Send("turn.on");

                for (var i = 0; i < 5; i++)
                {
                    context.Send("time");
                }

                context.Send("cancel");
            });
        }

        static Task Run(string xmldoc, ILogger logger, Action<ExecutionContext> action = null)
        {
            var metadata = new XmlModelMetadata(XDocument.Load(xmldoc));

            var context = new ExecutionContext(metadata);

            context.Logger = logger;

            var interpreter = new Interpreter();

            var runTask = interpreter.Run(context);

            if (action != null)
            {
                action.Invoke(context);
            }

            return runTask;
        }
    }
}
