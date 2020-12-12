using DurableTask.Emulator;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    public delegate (IExecutionContext, Func<Task>) ScaffoldFactoryDelegate(IRootStateMetadata metadata,
                                                                            CancellationToken token,
                                                                            ILogger logger);

    [AttributeUsage(AttributeTargets.Method)]
    public class TestScaffoldAttribute : Attribute, ITestDataSource
    {
        public static TimeSpan ExecutionTimeout => Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(1);

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            yield return new object[]
            {
                (ScaffoldFactoryDelegate) ((machine, cancelToken, logger) =>
                {
                    var context = new ExecutionContext(machine, logger);

                    var interpreter = new Interpreter();

                    return (context, () => interpreter.RunAsync(context, cancelToken));
                }),
                "Lite"
            };

            yield return new object[]
            {
                (ScaffoldFactoryDelegate) ((machine, cancelToken, logger) =>
                {
                    var context = new Durable.ExecutionContext(machine, logger);

                    var emulator = new LocalOrchestrationService();

                    var interpreter = new Durable.Interpreter(emulator);

                    return (context, () => interpreter.RunAsync(context, ExecutionTimeout, cancelToken));
                }),
                "Durable"
            };
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            return $"{methodInfo.Name}-{data[1]}";
        }
    }
}
