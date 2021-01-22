using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class ErrorTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task ErrorMessateNotHandled(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .State("state1")
                                        .OnEntry
                                            .Execute(_ => bool.Parse("hello"))._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            Assert.IsTrue(true, "Internal error message not handled. Statechart processing successful.");
        }

        [TestMethod]
        [TestScaffold]
        public async Task ErrorMessageHandled(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .State("state1")
                                        .OnEntry
                                            .Execute(_ => bool.Parse("hello"))._
                                        .Transition
                                            .Message("error.*")
                                            .Assign("error", "found it")
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            var error = context.Data["error"];

            Assert.AreEqual(error, "found it");
        }

        [TestMethod]
        [TestScaffold]
        public async Task FailFast(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .FailFast(true)
                                    .State("state1")
                                        .OnEntry
                                            .Execute(_ => bool.Parse("hello"))._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAsync();

            await Assert.ThrowsExceptionAsync<ExecutionException>(() => context.WaitForCompletionAsync());
        }
    }
}
