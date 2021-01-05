﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            static void action(dynamic data) => throw new Exception("boo!");

            var machine = StateChart.Define("test")
                                    .State("state1")
                                        .OnEntry
                                            .Execute(action)._
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
            static void action(dynamic data) => throw new Exception("boo!");

            static object getContent(dynamic data) => data._event.Content;

            var machine = StateChart.Define("test")
                                    .State("state1")
                                        .OnEntry
                                            .Execute(action)._
                                        .Transition
                                            .Message("error.*")
                                            .Target("errorState")._._
                                    .State("errorState")
                                        .OnEntry
                                            .Assign("err", getContent)._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            var error = (Exception) context.Data["err"];

            Assert.IsInstanceOfType(error, typeof(ExecutionException));
            Assert.IsNotNull(error.InnerException);
            Assert.AreEqual("boo!", error.InnerException.Message);
        }

        [TestMethod]
        [TestScaffold]
        public async Task FailFast(ScaffoldFactoryDelegate factory, string _)
        {
            static void action(dynamic data) => throw new Exception("boo!");

            var machine = StateChart.Define("test")
                                    .FailFast(true)
                                    .State("state1")
                                        .OnEntry
                                            .Execute(action)._
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
