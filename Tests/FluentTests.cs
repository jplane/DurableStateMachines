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
    public class FluentTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task SimpleTransition(ScaffoldFactoryDelegate factory, string _)
        {
            var x = 1;

            var machine = StateChart.Define("test")
                                    .AtomicState("state1")
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

            var scaffold = factory(machine, CancellationToken.None, null);

            var context = scaffold.Item1;

            await scaffold.Item2();

            Assert.AreEqual(3, x);
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpPost(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync(uri));

            var x = 5;

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Body(new { value = x })
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var scaffold = factory(machine, CancellationToken.None, null);

            var context = scaffold.Item1;

            await scaffold.Item2();

            var json = await listenerTask;

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(int) });

            Assert.AreEqual(5, content.value);
        }

        [TestMethod]
        [TestScaffold]
        public async Task Delay(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync(uri));

            var now = DateTimeOffset.UtcNow;

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Delay(TimeSpan.FromSeconds(5))
                                                .Body(_ => new { value = DateTimeOffset.UtcNow })
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var scaffold = factory(machine, CancellationToken.None, null);

            var context = scaffold.Item1;

            await scaffold.Item2();

            var json = await listenerTask;

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(DateTimeOffset) });

            Assert.IsTrue(content.value >= now + TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpGet(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.JsonResultAsync(uri, new { value = 43 }));

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpGet()
                                                .Url(uri)
                                                .ResultLocation("x")
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var scaffold = factory(machine, CancellationToken.None, null);

            var context = scaffold.Item1;

            await Task.WhenAll(scaffold.Item2(), listenerTask);

            var json = (string)context["x"];

            Assert.IsNotNull(json);

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleParentChild(ScaffoldFactoryDelegate factory, string _)
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
                                            .Definition(innerMachine)
                                            .Attach()
                                        .Transition()
                                            .Message("done.invoke.*")
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var scaffold = factory(machine, CancellationToken.None, null);

            var context = scaffold.Item1;

            await scaffold.Item2();

            Assert.AreEqual(3, x);
        }
    }
}
