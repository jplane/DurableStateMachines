using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using DSM.Common;
using DSM.Metadata.Actions;
using DSM.Metadata.States;
using System.Threading.Tasks;

namespace DSM.Tests
{
    [TestClass]
    public partial class HttpActionTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task HttpPost(ScaffoldFactoryDelegate factory, string _)
        {
            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync("http://localhost:4444/"));

            var machine = new StateMachine<bool>
            {
                Id = "test",
                States =
                {
                    new AtomicState<bool>
                    {
                        Id = "state1",
                        OnEntry = new OnEntryExit<bool>
                        {
                            Actions =
                            {
                                new SendMessage<bool>
                                {
                                    Id = "test-post",
                                    ActivityType = "http-post",
                                    Configuration = new HttpSendMessageConfiguration
                                    {
                                        Uri = "http://localhost:4444/",
                                        Content = new { value = 5 }
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<bool> { Target = "alldone" }
                        }
                    },
                    new FinalState<bool>
                    {
                        Id = "alldone"
                    }
                }
            };

            var tuple = factory(machine, null, Logger);

            var context = tuple.Item1;

            await context.RunAsync(true);

            var jsonResult = await listenerTask;

            var content = JsonConvert.DeserializeAnonymousType(jsonResult, new { value = default(int) });

            Assert.AreEqual(5, content.value);
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpGet(ScaffoldFactoryDelegate factory, string _)
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.JsonResultAsync(uri, new { value = 43 }));

            var machine = new StateMachine<(string x, string y)>
            {
                Id = "test",
                States =
                {
                    new AtomicState<(string x, string y)>
                    {
                        Id = "state1",
                        OnEntry = new OnEntryExit<(string x, string y)>
                        {
                            Actions =
                            {
                                new Query<(string x, string y)>
                                {
                                    ActivityType = "http-get",
                                    AssignTo = d => d.x,
                                    Configuration = new HttpQueryConfiguration
                                    {
                                        Uri = "http://localhost:4444/"
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(string x, string y)> { Target = "alldone" }
                        }
                    },
                    new FinalState<(string x, string y)>
                    {
                        Id = "alldone"
                    }
                }
            };

            (string x, string y) data = ("", "");

            var tuple = factory(machine, null, Logger);

            var context = tuple.Item1;

            var task = context.RunAsync(data);

            await Task.WhenAll(task, listenerTask);

            var content = JsonConvert.DeserializeAnonymousType(task.Result.x, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }
    }
}
