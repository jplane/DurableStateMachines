using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Metadata.Fluent.States;
using StateChartsDotNet.Metadata.Fluent.Queries.HttpGet;
using StateChartsDotNet.Metadata.Fluent.Services.HttpPost;
using System.Threading.Tasks;
using System;
using System.Threading;
using StateChartsDotNet.Common.Model.States;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class ParentChildTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task IsolatedExecution(ScaffoldFactoryDelegate factory, string _)
        {
            var x = 1;

            var innerMachine = StateChart.Define("inner")
                                         .AtomicState("innerState1")
                                             .OnEntry()
                                                 .Execute(_ => x += 1)
                                                 .Attach()
                                             .OnExit()
                                                 .Execute(_ => x += 1)
                                                 .Attach()
                                             .Transition()
                                                 .Target("alldone")
                                                 .Attach()
                                             .Attach()
                                         .FinalState("alldone")
                                             .Attach();

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .ExecutionMode(ChildStateChartExecutionMode.Isolated)
                                            .Definition(innerMachine)
                                            .Attach()
                                        .Transition()
                                            .Message("done.invoke.*")
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            Assert.AreEqual(3, x);
        }

        [TestMethod]
        [TestScaffold]
        public async Task InlineExecution(ScaffoldFactoryDelegate factory, string _)
        {
            var x = 1;

            var innerMachine = StateChart.Define("inner")
                                         .AtomicState("innerState1")
                                             .OnEntry()
                                                 .Execute(_ => x += 1)
                                                 .Attach()
                                             .OnExit()
                                                 .Execute(_ => x += 1)
                                                 .Attach()
                                             .Transition()
                                                 .Target("alldone")
                                                 .Attach()
                                             .Attach()
                                         .FinalState("alldone")
                                             .Attach();

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .ExecutionMode(ChildStateChartExecutionMode.Inline)
                                            .Definition(innerMachine)
                                            .Attach()
                                        .Transition()
                                            .Message("done.invoke.*")
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            Assert.AreEqual(3, x);
        }
    }
}
