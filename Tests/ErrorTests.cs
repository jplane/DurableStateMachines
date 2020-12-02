using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class ErrorTests
    {
        [TestMethod]
        public async Task ErrorMessateNotHandled()
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

            var context = new ExecutionContext(machine);

            var interpreter = new Interpreter();

            await interpreter.RunAsync(context);

            Assert.IsTrue(true, "Internal error message not handled. Statechart processing successful.");
        }

        [TestMethod]
        public async Task ErrorMessageHandled()
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

            var context = new ExecutionContext(machine);

            var interpreter = new Interpreter();

            await interpreter.RunAsync(context);

            var ex = (Exception) context["err"];

            Assert.IsNotNull(ex);
            Assert.IsInstanceOfType(ex, typeof(ExecutionException));
            Assert.IsNotNull(ex.InnerException);

            Assert.AreEqual("boo!", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task FailFast()
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

            var context = new ExecutionContext(machine);

            var interpreter = new Interpreter();

            await Assert.ThrowsExceptionAsync<ExecutionException>(() => interpreter.RunAsync(context));
        }
    }
}
