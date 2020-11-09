using CoreEngine;
using System;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Dynamic;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using CoreEngine.ModelProvider.Xml;

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
            return Run("microwave.xml", logger, interpreter =>
            {
                interpreter.Context.Enqueue("turn.on");

                for (var i = 0; i < 5; i++)
                {
                    interpreter.Context.Enqueue("time");
                }

                interpreter.Context.Enqueue("cancel");
            });
        }

        static Task Run(string xmldoc, ILogger logger, Action<Interpreter> action = null)
        {
            var metadata = new XmlModelMetadata(XDocument.Load(xmldoc));

            var interpreter = new Interpreter(metadata);

            interpreter.Context.Logger = logger;

            var task = interpreter.Run();

            action?.Invoke(interpreter);

            return task;
        }
    }
}
