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
    public delegate (IInstanceManager, IExecutionContext) ScaffoldFactoryDelegate(IRootStateMetadata metadata, ILogger logger);

    [AttributeUsage(AttributeTargets.Method)]
    public class TestScaffoldAttribute : Attribute, ITestDataSource
    {
        public static TimeSpan ExecutionTimeout => Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(1);

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            yield return new object[]
            {
                (ScaffoldFactoryDelegate) ((machine, logger) =>
                {
                    var context = new ExecutionContext(machine, logger);
                    return (context, context);
                }),
                "Lite"
            };

            yield return new object[]
            {
                (ScaffoldFactoryDelegate) ((machine, logger) =>
                {
                    var emulator = new LocalOrchestrationService();

                    var context = new Durable.ExecutionContext(machine, emulator, ExecutionTimeout, logger);

                    return (context, context);
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
