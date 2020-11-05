using CoreEngine;
using System;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Dynamic;
using Microsoft.CodeAnalysis.Scripting;

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
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.AutoFlush = true;

            var xml = XDocument.Load("machine.xml");

            var interpreter = new Interpreter(xml);

            var task = Task.Factory.StartNew(() => interpreter.Run());

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
