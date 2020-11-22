using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateChartsDotNet.CoreEngine;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States;
using System;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class FluentTests
    {
        [TestMethod]
        public async Task SimpleTransition()
        {
            var x = 1;

            var root = RootStateMetadata.Create("test")
                                        .WithAtomicState("state1")
                                            .WithOnEntry()
                                                .WithScript(_ => x += 1)
                                                .Attach()
                                            .WithOnExit()
                                                .WithScript(_ => x += 1)
                                                .Attach()
                                            .WithTransition()
                                                .WithTarget("alldone")
                                                .Attach()
                                            .Attach()
                                        .WithFinalState("alldone")
                                        .Attach();

            var context = new ExecutionContext(root);

            var interpreter = new Interpreter();

            await interpreter.Run(context);

            Assert.AreEqual(3, x);
        }
    }
}
