using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Metadata.Fluent.States;
using StateChartsDotNet.Metadata.Fluent.Queries.HttpGet;
using StateChartsDotNet.Metadata.Fluent.Services.HttpPost;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class FluentTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task SimpleTransition(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .DataInit("x", 1)
                                    .State("state1")
                                        .OnEntry
                                            .Assign("x", data => ((int) data["x"]) + 1)._
                                        .OnExit
                                            .Assign("x", data => ((int) data["x"]) + 1)._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            var result = Convert.ToInt32(context.Data["x"]);

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleTransitionWithScript(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .State("state1")
                                        .OnEntry
                                            .Execute(data => data.Add("x", 3))._
                                        .OnExit
                                            .Execute(data => data.Add("y", 4))._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            var x = Convert.ToInt32(context.Data["x"]);
            Assert.AreEqual(3, x);

            var y = Convert.ToInt32(context.Data["y"]);
            Assert.AreEqual(4, y);
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpPost(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync(uri));

            var machine = StateChart.Define("httptest")
                                    .State("state1")
                                        .OnEntry
                                            .HttpPost()
                                                .Url(uri)
                                                .Body(new { value = 5 })._._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

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

            var machine = StateChart.Define("httptest")
                                    .State("state1")
                                        .OnEntry
                                            .Execute(data => data.Add("x", new { value = DateTimeOffset.UtcNow }))
                                            .HttpPost()
                                                .Url(uri)
                                                .Delay(TimeSpan.FromSeconds(5))
                                                .BodyExpr(_ => new { value = DateTimeOffset.UtcNow })._._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

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
                                    .State("state1")
                                        .OnEntry
                                            .HttpGet()
                                                .Url(uri)
                                                .ResultLocation("x")._._
                                        .Transition
                                            .Target("alldone")._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            var task = context.StartAndWaitForCompletionAsync();

            await Task.WhenAll(task, listenerTask);

            var json = (string) context.Data["x"];

            Assert.IsNotNull(json);

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleParallel(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = StateChart.Define("test")
                                    .DataInit("x", 1)
                                    .ParallelState("parallel")
                                        .State("state1")
                                            .OnEntry
                                                .Assign("x", data => ((int)data["x"]) + 1)._
                                            .OnExit
                                                .Assign("x", data => ((int)data["x"]) + 1)._
                                            .Transition
                                                .Target("alldone")._._
                                        .State("state2")
                                            .OnEntry
                                                .Assign("x", data => ((int)data["x"]) + 1)._
                                            .OnExit
                                                .Assign("x", data => ((int)data["x"]) + 1)._
                                            .Transition
                                                .Target("alldone")._._._
                                    .FinalState("alldone")._;

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            var result = Convert.ToInt32(context.Data["x"]);

            Assert.AreEqual(5, result);
        }
    }
}
