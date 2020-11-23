using EmbedIO;
using EmbedIO.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet;
using StateChartsDotNet.Metadata.Fluent.States;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class SendMessageTests
    {
        private static async Task<string> RunHttpListener(string uri)
        {
            var completion = new TaskCompletionSource<string>();

            RequestHandlerCallback callback = async ctx =>
            {
                completion.SetResult(await ctx.GetRequestBodyAsStringAsync());
            };

            var server = new WebServer(options => options.WithUrlPrefix(uri)
                                                         .WithMode(HttpListenerMode.EmbedIO))
                                                         .WithModule(new ActionModule("/", HttpVerbs.Any, callback));

            using (server)
            {
                server.RunAsync();

                return await completion.Task;
            }
        }

        [TestMethod]
        public async Task SimpleHttp()
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => RunHttpListener(uri));

            var x = 1;

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .SendMessage()
                                                .Type("http-post")
                                                .Target(uri)
                                                .Content(new { value = x })
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

            Assert.AreEqual(1, content.value);
        }
    }
}
