using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Metadata.Fluent.States;
using StateChartsDotNet.Metadata.Fluent.Queries.HttpGet;
using StateChartsDotNet.Metadata.Fluent.Services.HttpPost;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class ParentChildTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task AutoforwardFalse(ScaffoldFactoryDelegate factory, string _)
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
                                            .Autoforward(false)
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

            var instanceMgr = tuple.Item1;

            var context = tuple.Item2;

            await instanceMgr.StartAndWaitForCompletionAsync();

            Assert.AreEqual(3, x);
        }

        [TestMethod]
        [TestScaffold]
        public async Task AutoforwardTrue(ScaffoldFactoryDelegate factory, string _)
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
                                            .Autoforward(true)
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

            var instanceMgr = tuple.Item1;

            var context = tuple.Item2;

            await instanceMgr.StartAndWaitForCompletionAsync();

            Assert.AreEqual(3, x);
        }
    }
}
