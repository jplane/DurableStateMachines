using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.Tests
{
    public abstract class TestBase
    {
        protected static ILogger Logger;

        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void Init(TestContext context)
        {
            var timeout = TestScaffoldAttribute.ExecutionTimeout.Add(TimeSpan.FromMinutes(1));

            context.CancellationTokenSource.CancelAfter(timeout);

            var loggerFactory = LoggerFactory.Create(
                                    builder => builder.AddFilter("SCDNTests", level => true).AddDebug());

            Logger = loggerFactory.CreateLogger("SCDNTests");
        }
    }
}
