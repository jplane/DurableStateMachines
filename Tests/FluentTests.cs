using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Metadata.Fluent.States;
using StateChartsDotNet.Queries.HttpGet;
using StateChartsDotNet.Services.HttpPost;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class FluentTests
    {
        [TestMethod]
        public async Task SimpleTransition()
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

            var context = new ExecutionContext(machine);

            var interpreter = new Interpreter();

            await interpreter.RunAsync(context);

            Assert.AreEqual(3, x);
        }

        [TestMethod]
        public async Task HttpPost()
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

            var context = new ExecutionContext(machine);

            var interpreter = new Interpreter();

            await interpreter.RunAsync(context);

            var json = await listenerTask;

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(int) });

            Assert.AreEqual(5, content.value);
        }

        [TestMethod]
        public async Task HttpGet()
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

            var context = new ExecutionContext(machine);

            var interpreter = new Interpreter();

            await Task.WhenAll(interpreter.RunAsync(context), listenerTask);

            var json = (string) context["x"];

            Assert.IsNotNull(json);

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }

        [TestMethod]
        public async Task SimpleParentChild()
        {
            var x = 1;

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .Definition(StateChart.Define("inner")
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
                                                                      .Attach())
                                            .Attach()
                                        .Transition()
                                            .Message("done.invoke.*")
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var context = new ExecutionContext(machine);

            var interpreter = new Interpreter();

            await interpreter.RunAsync(context);

            Assert.AreEqual(3, x);
        }
    }
}
