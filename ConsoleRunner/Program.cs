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

            var xml = XDocument.Load("machine.xml");

            var interpreter = new Interpreter(xml);

            interpreter.Context.Logger = loggerFactory.CreateLogger("StateChart");

            var task = interpreter.Run();

            interpreter.Context.Enqueue("turn.on");

            for (var i = 0; i < 5; i++)
            {
                interpreter.Context.Enqueue("time");
            }

            interpreter.Context.Enqueue("cancel");

            task.Wait();
        }
    }
}
