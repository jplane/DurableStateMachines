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
            static object getValue(dynamic data) => data.x + 1;

            var machine = StateChart.Define("test")
                                    .Datamodel()
                                        .DataInit()
                                            .Id("x").Value(1).Attach()
                                        .Attach()
                                    .AtomicState("state1")
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
                                        .Attach();

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            var result = Convert.ToInt32(context.Data["x"]);

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpPost(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync(uri));

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

            await context.StartAndWaitForCompletionAsync();

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

            static object getValue(dynamic data) => new { value = DateTimeOffset.UtcNow };

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Delay(TimeSpan.FromSeconds(5))
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

            await context.StartAndWaitForCompletionAsync();

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

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            var task = context.StartAndWaitForCompletionAsync();

            await Task.WhenAll(task, listenerTask);

            var json = (string) context.Data["x"];

            Assert.IsNotNull(json);

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }
    }
}
