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
            static object getValue(dynamic data) => data.x + 1;

            var innerMachine = StateChart.Define("inner")
                                         .AtomicState("innerState1")
                                             .Datamodel()
                                                 .DataInit()
                                                     .Id("x").Value(1).Attach()
                                                 .Attach()
                                             .OnEntry()
                                                 .Assign()
                                                    .Location("x").Value(getValue).Attach()
                                                 .Attach()
                                             .OnExit()
                                                 .Assign()
                                                    .Location("x").Value(getValue).Attach()
                                                 .Attach()
                                             .Transition()
                                                 .Target("alldone")
                                                 .Attach()
                                             .Attach()
                                         .FinalState("alldone")
                                             .Param("x").Location("x").Attach()
                                             .Attach();

            static object getEventValue(dynamic data) => data._event.Parameters["x"];

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .ExecutionMode(ChildStateChartExecutionMode.Isolated)
                                            .Definition(innerMachine)
                                            .Assign()
                                                .Location("x").Value(getEventValue).Attach()
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

            var result = Convert.ToInt32(context.Data["x"]);

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        [TestScaffold]
        public async Task InlineExecution(ScaffoldFactoryDelegate factory, string _)
        {
            static object getValue(dynamic data) => data.x + 1;

            var innerMachine = StateChart.Define("inner")
                                         .AtomicState("innerState1")
                                             .Datamodel()
                                                 .DataInit()
                                                     .Id("x").Value(1).Attach()
                                                 .Attach()
                                             .OnEntry()
                                                 .Assign()
                                                    .Location("x").Value(getValue).Attach()
                                                 .Attach()
                                             .OnExit()
                                                 .Assign()
                                                    .Location("x").Value(getValue).Attach()
                                                 .Attach()
                                             .Transition()
                                                 .Target("alldone")
                                                 .Attach()
                                             .Attach()
                                         .FinalState("alldone")
                                             .Param("x").Location("x").Attach()
                                             .Attach();

            static object getEventValue(dynamic data) => data._event.Parameters["x"];

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .ExecutionMode(ChildStateChartExecutionMode.Inline)
                                            .Definition(innerMachine)
                                            .Assign()
                                                .Location("x").Value(getEventValue).Attach()
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

            var result = Convert.ToInt32(context.Data["x"]);

            Assert.AreEqual(3, result);
        }
    }
}
