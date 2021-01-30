using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace StateChartsDotNet.Tests
{
    public delegate (IExecutionContext, CancellationTokenSource)
            ScaffoldFactoryDelegate(IStateChartMetadata metadata, object data, Func<string, IStateChartMetadata> lookup, ILogger logger);

    [AttributeUsage(AttributeTargets.Method)]
    public class TestScaffoldAttribute : Attribute, ITestDataSource
    {
        public static TimeSpan ExecutionTimeout => Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(1);

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            yield return new object[]
            {
                (ScaffoldFactoryDelegate) ((machine, data, lookup, logger) =>
                {
                    var cts = new CancellationTokenSource();

                    var context = new ExecutionContext(machine, data, cts.Token, lookup, false, logger);
                    
                    return (context, cts);
                }),
                "Lite"
            };

            //yield return new object[]
            //{
            //    (ScaffoldFactoryDelegate) ((machine, logger) =>
            //    {
            //        var cts = new CancellationTokenSource();

            //        var emulator = new LocalOrchestrationService();

            //        var storage = new Durable.InMemoryOrchestrationStorage();

            //        var context = new Durable.ExecutionContext(machine, emulator, storage, cts.Token, ExecutionTimeout, logger);

            //        return (context, cts);
            //    }),
            //    "Durable"
            //};
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            return $"{methodInfo.Name}-{data[1]}";
        }
    }
}
