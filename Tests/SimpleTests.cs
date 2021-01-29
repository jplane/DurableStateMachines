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
        [TestMethod]
        [TestScaffold]
        public async Task Foreach(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine
            {
                Id = "test",
                InitialState = "loop",
                States =
                {
                    new AtomicState
                    {
                        Id = "loop",
                        OnEntry = new OnEntryExit
                        {
                            Actions =
                            {
                                new Foreach
                                {
                                    CurrentItemLocation = "arrayItem",
                                    ValueExpression = "items",
                                    Actions =
                                    {
                                        new Assign { Location = "sum", ValueExpression = "sum + arrayItem" },
                                        new Log { MessageExpression = "\"item = \" + arrayItem" }
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition
                            {
                                ConditionExpression = "sum >= 15",
                                Targets = { "done" }
                            }
                        }
                    },
                    new FinalState
                    {
                        Id = "done",
                        OnEntry = new OnEntryExit
                        {
                            Actions =
                            {
                                new Log { MessageExpression = "\"item = \" + arrayItem" }
                            }
                        }
                    }
                }
            };

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            context.Data["items"] = new[] { 1, 2, 3, 4, 5 };

            context.Data["sum"] = 0;

            await context.StartAndWaitForCompletionAsync();

            Assert.AreEqual(15, Convert.ToInt32(context.Data["sum"]));
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpPost(ScaffoldFactoryDelegate factory, string _)
        {
            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync("http://localhost:4444/"));

            var machine = new StateMachine
            {
                Id = "test",
                States =
                {
                    new AtomicState
                    {
                        Id = "state1",
                        OnEntry = new OnEntryExit
                        {
                            Actions =
                            {
                                new SendMessage
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
                            new Transition { Targets = { "alldone" } }
                        }
                    },
                    new FinalState
                    {
                        Id = "alldone"
                    }
                }
            };

            var tuple = factory(machine, Logger);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

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

            var machine = new StateMachine
            {
                Id = "test",
                States =
                {
                    new AtomicState
                    {
                        Id = "state1",
                        OnEntry = new OnEntryExit
                        {
                            Actions =
                            {
                                new Query
                                {
                                    ActivityType = "http-get",
                                    ResultLocation = "x",
                                    Configuration = new HttpQueryConfiguration
                                    {
                                        Uri = "http://localhost:4444/"
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition { Targets = { "alldone" } }
                        }
                    },
                    new FinalState
                    {
                        Id = "alldone"
                    }
                }
            };

            var tuple = factory(machine, Logger);

            var context = tuple.Item1;

            var task = context.StartAndWaitForCompletionAsync();

            await Task.WhenAll(task, listenerTask);

            var jsonResult = (string) context.Data["x"];

            Assert.IsNotNull(jsonResult);

            var content = JsonConvert.DeserializeAnonymousType(jsonResult, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleParentChild(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine
            {
                Id = "outer",
                States =
                {
                    new AtomicState
                    {
                        Id = "state1",
                        Invokes =
                        {
                            new InvokeStateChart
                            {
                                Id = "an-invoke",
                                Parameters =
                                {
                                    new Param
                                    {
                                        Name = "x",
                                        Location = "x"
                                    }
                                },
                                ResultLocation = "innerResults",
                                Definition = new StateMachine
                                {
                                    Id = "inner",
                                    States =
                                    {
                                        new AtomicState
                                        {
                                            Id = "innerState1",
                                            OnEntry = new OnEntryExit
                                            {
                                                Actions =
                                                {
                                                    new Assign { Location = "x", ValueExpression = "x * 2" }
                                                }
                                            },
                                            Transitions =
                                            {
                                                new Transition { Targets = { "alldone" } }
                                            }
                                        },
                                        new FinalState
                                        {
                                            Id = "alldone"
                                        }
                                    }
                                },
                                CompletionActions =
                                {
                                    new Assign { Location = "innerX", ValueExpression = "innerResults[\"x\"]" }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition 
                            {
                                Messages = { "done.invoke.*" },
                                Targets = { "alldone" }
                            }
                        }
                    },
                    new FinalState
                    {
                        Id = "alldone"
                    }
                }
            };

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            context.Data["x"] = 5;

            await context.StartAndWaitForCompletionAsync();

            var x = Convert.ToInt32(context.Data["x"]);

            Assert.AreEqual(5, x);

            var innerX = Convert.ToInt32(context.Data["innerX"]);

            Assert.AreEqual(10, innerX);
        }
    }
}
