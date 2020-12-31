using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Metadata.Fluent.States;
using StateChartsDotNet.Metadata.Fluent.Queries.HttpGet;
using StateChartsDotNet.Metadata.Fluent.Services.HttpPost;
using System.Threading.Tasks;
using System;
using System.Threading;
using StateChartsDotNet.Common.Model.States;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class CancelTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task ExternalByMessage(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .AtomicState("state1").Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAsync();

            await Task.Delay(500);

            await context.SendStopMessageAsync();

            await context.WaitForCompletionAsync();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [TestScaffold]
        public async Task ExternalByToken(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .AtomicState("state1").Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            var cts = tuple.Item2;

            await context.StartAsync();

            await Task.Delay(500);

            cts.Cancel();

            await context.WaitForCompletionAsync();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpPostByMessage(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.DelayEchoAsync(uri, TimeSpan.FromSeconds(10)));

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Body(new { value = 5 })
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAsync();

            await Task.Delay(2000);

            await context.SendStopMessageAsync();

            await context.WaitForCompletionAsync();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpPostByToken(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.DelayEchoAsync(uri, TimeSpan.FromSeconds(10)));

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Body(new { value = 5 })
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var start = DateTimeOffset.UtcNow;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            var cts = tuple.Item2;

            await context.StartAsync();

            await Task.Delay(2000);

            cts.Cancel();

            await context.WaitForCompletionAsync();

            var elapsed = DateTimeOffset.UtcNow - start;

            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        [TestScaffold]
        public async Task DelayByMessage(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync(uri));

            var now = DateTimeOffset.UtcNow;

            static object getValue(dynamic data) => new { value = DateTimeOffset.UtcNow };

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Delay(TimeSpan.FromSeconds(10))
                                                .Body(getValue)
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAsync();

            await Task.Delay(2000);

            await context.SendStopMessageAsync();

            await context.WaitForCompletionAsync();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [TestScaffold]
        public async Task DelayByToken(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var now = DateTimeOffset.UtcNow;

            static object getValue(dynamic data) => new { value = DateTimeOffset.UtcNow };

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Delay(TimeSpan.FromSeconds(10))
                                                .Body(getValue)
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var start = DateTimeOffset.UtcNow;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            var cts = tuple.Item2;

            await context.StartAsync();

            await Task.Delay(2000);

            cts.Cancel();

            await context.WaitForCompletionAsync();

            var elapsed = DateTimeOffset.UtcNow - start;

            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleParentChildByMessage(ScaffoldFactoryDelegate factory, string _)
        {
            var innerMachine = StateChart.Define("inner")
                                         .AtomicState("innerState1").Attach();

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .Definition(innerMachine)
                                            .Attach()
                                        .Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAsync();

            await Task.Delay(2000);

            await context.SendStopMessageAsync();

            await context.SendStopMessageAsync();

            await context.WaitForCompletionAsync();

            Assert.IsTrue(true);
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleParentChildByToken(ScaffoldFactoryDelegate factory, string _)
        {
            var innerMachine = StateChart.Define("inner")
                                         .AtomicState("innerState1").Attach();

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .Definition(innerMachine)
                                            .Attach()
                                        .Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            var cts = tuple.Item2;

            await context.StartAsync();

            await Task.Delay(2000);

            cts.Cancel();

            await context.WaitForCompletionAsync();

            Assert.IsTrue(true);
        }
    }
}
