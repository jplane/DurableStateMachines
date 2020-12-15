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
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .Execute(_ => throw new Exception("boo!"))
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var tuple = factory(machine, null);

            var instanceMgr = tuple.Item1;

            await instanceMgr.StartAsync();

            await instanceMgr.WaitForCompletionAsync();

            Assert.IsTrue(true, "Internal error message not handled. Statechart processing successful.");
        }

        [TestMethod]
        [TestScaffold]
        public async Task ErrorMessageHandled(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .Execute(_ => throw new Exception("boo!"))
                                            .Attach()
                                        .Transition()
                                            .Message("error.*")
                                            .Target("errorState")
                                            .Attach()
                                        .Attach()
                                    .AtomicState("errorState")
                                        .OnEntry()
                                            .Assign()
                                                .Location("err")
                                                .Value(data => data._event.Content)
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var tuple = factory(machine, null);

            var instanceMgr = tuple.Item1;

            var context = tuple.Item2;

            await instanceMgr.StartAsync();

            await instanceMgr.WaitForCompletionAsync();

            var error = (Exception) context.Data["err"];

            Assert.IsInstanceOfType(error, typeof(ExecutionException));
            Assert.IsNotNull(error.InnerException);
            Assert.AreEqual("boo!", error.InnerException.Message);
        }

        [TestMethod]
        [TestScaffold]
        public async Task FailFast(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .FailFast(true)
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .Execute(_ => throw new Exception("boo!"))
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var tuple = factory(machine, null);

            var instanceMgr = tuple.Item1;

            var context = tuple.Item2;

            await instanceMgr.StartAsync();

            await Assert.ThrowsExceptionAsync<ExecutionException>(() => instanceMgr.WaitForCompletionAsync());
        }
    }
}
