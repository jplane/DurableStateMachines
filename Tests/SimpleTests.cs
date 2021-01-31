using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Metadata.Execution;
using StateChartsDotNet.Metadata.States;
using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class SimpleTests : TestBase
    {
        public class TestData
        {
            public int[] Items;
            public int Sum;
            public int ArrayItem;
        }

        [TestMethod]
        [TestScaffold]
        public async Task Foreach(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine<TestData>
            {
                Id = "test",
                InitialState = "loop",
                States =
                {
                    new AtomicState<TestData>
                    {
                        Id = "loop",
                        OnEntry = new OnEntryExit<TestData>
                        {
                            Actions =
                            {
                                new Foreach<TestData>
                                {
                                    CurrentItemTarget = d => d.ArrayItem,
                                    ValueFunction = data => data.Items,
                                    Actions =
                                    {
                                        new Assign<TestData> { Target = d => d.Sum, ValueFunction = d => d.Sum + d.ArrayItem },
                                        new Log<TestData> { MessageFunction = d => $"item = {d.ArrayItem}" }
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<TestData>
                            {
                                ConditionFunction = d => d.Sum >= 15,
                                Targets = { "done" }
                            }
                        }
                    },
                    new FinalState<TestData>
                    {
                        Id = "done",
                        OnEntry = new OnEntryExit<TestData>
                        {
                            Actions =
                            {
                                new Log<TestData> { MessageFunction = d => $"item = {d.ArrayItem}" }
                            }
                        }
                    }
                }
            };

            var data = new TestData
            {
                Items = new[] { 1, 2, 3, 4, 5 },
                Sum = 0
            };

            var tuple = factory(machine, null, null);

            var context = tuple.Item1;

            await context.StartAsync(data);

            Assert.AreEqual(15, data.Sum);
        }

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
                            new Transition<bool> { Targets = { "alldone" } }
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

            await context.StartAsync(true);

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
                                    ResultTarget = d => d.x,
                                    Configuration = new HttpQueryConfiguration
                                    {
                                        Uri = "http://localhost:4444/"
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(string x, string y)> { Targets = { "alldone" } }
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

            var task = context.StartAsync(data);

            await Task.WhenAll(task, listenerTask);

            var content = JsonConvert.DeserializeAnonymousType(task.Result.x, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleParentChild(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine<(int x, (int x, int y) innerX)>
            {
                Id = "outer",
                States =
                {
                    new AtomicState<(int x, (int x, int y) innerX)>
                    {
                        Id = "state1",
                        Invokes =
                        {
                            new InvokeStateChart<(int x, (int x, int y) innerX)>
                            {
                                Id = "an-invoke",
                                StateMachineIdentifier = "inner",
                                DataFunction = d => (d.x, 0),
                                ResultTarget = d => d.innerX
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, (int x, int y) innerX)>
                            {
                                Messages = { "done.invoke.*" },
                                Targets = { "alldone" }
                            }
                        }
                    },
                    new FinalState<(int x, (int x, int y) innerX)>
                    {
                        Id = "alldone"
                    }
                }
            };

            var childMachine = new StateMachine<(int x, int y)>
            {
                Id = "inner",
                States =
                {
                    new AtomicState<(int x, int y)>
                    {
                        Id = "innerState1",
                        OnEntry = new OnEntryExit<(int x, int y)>
                        {
                            Actions =
                            {
                                new Assign<(int x, int y)> { Target = d => d.x, ValueFunction = d => d.x * 2 }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, int y)> { Targets = { "alldone" } }
                        }
                    },
                    new FinalState<(int x, int y)>
                    {
                        Id = "alldone"
                    }
                }
            };

            (int x, (int x, int y) innerX) data = (5, (0, 0));

            var tuple = factory(machine, _ => childMachine, null);

            var context = tuple.Item1;

            data = await context.StartAsync(data);

            Assert.AreEqual(5, data.x);

            Assert.AreEqual(10, data.innerX.x);
        }
    }
}
