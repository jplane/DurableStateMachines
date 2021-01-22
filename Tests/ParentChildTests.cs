using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Metadata.Fluent.States;
using StateChartsDotNet.Metadata.Fluent.Queries.HttpGet;
using StateChartsDotNet.Metadata.Fluent.Services.HttpPost;
using System.Threading.Tasks;
using System;
using System.Threading;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Messages;
using System.Collections;
using System.Collections.Generic;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class ParentChildTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task InlineExecution(ScaffoldFactoryDelegate factory, string _)
        {
            var innerMachine = StateChart.Define("inner")
                                         .State("innerState1")
                                             .DataInit("x", 1)
                                             .OnEntry
                                                .Assign("x", data => ((int)data["x"]) + 1)._
                                             .OnExit
                                                .Assign("x", data => ((int)data["x"]) + 1)._
                                             .Transition
                                                 .Target("alldone")._._
                                         .FinalState("alldone")
                                             .Param("x").Location("x")._._;

            var machine = StateChart.Define("outer")
                                    .State("outerState1")
                                        .InvokeStateChart
                                            .Id("example-invoke")
                                            .ExecutionMode(ChildStateChartExecutionMode.Inline)
                                            .ResultLocation("innerResults")
                                            .Definition(innerMachine)
                                            .Assign("outerX", data => ((IDictionary<string, object>) data["innerResults"])["x"])._
                                        .Transition
                                            .Message("done.invoke.*")
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            var result = Convert.ToInt32(context.Data["outerX"]);

            Assert.AreEqual(3, result);
        }
    }
}
