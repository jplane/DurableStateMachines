using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using DSM.Common;
using DSM.Metadata.Actions;
using DSM.Metadata.States;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace DSM.Tests
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
                                    CurrentItem = d => d.ArrayItem,
                                    ValueFunction = data => data.Items,
                                    Actions =
                                    {
                                        new Assign<TestData> { To = d => d.Sum, ValueFunction = d => d.Sum + d.ArrayItem },
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
                                Target = "done"
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

            await context.RunAsync(data);

            Assert.AreEqual(15, data.Sum);
        }

        [TestMethod]
        [TestScaffold]
        public async Task JsonSerialization(ScaffoldFactoryDelegate factory, string _)
        {
            //var machine = new StateMachine<TestData>
            //{
            //    Id = "test",
            //    InitialState = "loop",
            //    States =
            //    {
            //        new AtomicState<TestData>
            //        {
            //            Id = "loop",
            //            OnEntry = new OnEntryExit<TestData>
            //            {
            //                Actions =
            //                {
            //                    new Foreach<TestData>
            //                    {
            //                        CurrentItem = d => d.ArrayItem,
            //                        ValueFunction = data => data.Items,
            //                        Actions =
            //                        {
            //                            new Assign<TestData> { To = d => d.Sum, ValueFunction = d => d.Sum + d.ArrayItem },
            //                            new Log<TestData> { MessageFunction = d => $"item = {d.ArrayItem}" }
            //                        }
            //                    }
            //                }
            //            },
            //            Transitions =
            //            {
            //                new Transition<TestData>
            //                {
            //                    ConditionFunction = d => d.Sum >= 15,
            //                    Target = "done"
            //                }
            //            }
            //        },
            //        new FinalState<TestData>
            //        {
            //            Id = "done",
            //            OnEntry = new OnEntryExit<TestData>
            //            {
            //                Actions =
            //                {
            //                    new Log<TestData> { MessageFunction = d => $"item = {d.ArrayItem}" }
            //                }
            //            }
            //        }
            //    }
            //};

            var json = @"{
                           'id': 'test',
                           'states': [
                             {
                               'id': 'loop',
                               'type': 'atomic',
                               'onentry': {
                                 'actions': [
                                   {
                                     'type': 'foreach`1',
                                     'currentitemlocation': 'arrayItem',
                                     'valueexpression': 'items',
                                     'actions': [
                                       {
                                         'type': 'assign`1',
                                         'target': 'sum',
                                         'valueexpression': 'sum + arrayItem'
                                       },
                                       {
                                         'type': 'log`1',
                                         'messageexpression': '""item = "" + arrayItem'
                                       }
                                     ]
                                   }
                                 ]
                               },
                               'transitions': [
                                 {
                                   'conditionexpression': 'sum >= 15',
                                   'target': 'done'
                                 }
                               ]
                             },
                             {
                               'id': 'done',
                               'type': 'final',
                               'onentry': {
                                 'actions': [
                                   {
                                     'type': 'log`1',
                                     'messageexpression': '""item = "" + arrayItem'
                                   }
                                 ]
                               }
                             }
                           ]
                         }";

            var machine = StateMachine<Dictionary<string, object>>.FromJson(json);

            Assert.IsNotNull(machine);

            var data = new Dictionary<string, object>
            {
                { "items", new [] { 1, 2, 3, 4, 5 } },
                { "sum", 0 }
            };

            var tuple = factory(machine, null, null);

            var context = tuple.Item1;

            await context.RunAsync(data);

            var sum = (int) data["sum"];

            Assert.IsTrue(sum >= 15);
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
                        OnEntry = new OnEntryExit<(int x, (int x, int y) innerX)>
                        {
                            Actions =
                            {
                                new InvokeStateMachine<(int x, (int x, int y) innerX)>
                                {
                                    Id = "an-invoke",
                                    StateMachineIdentifier = "inner",
                                    InputFunction = d => (d.x, 0),
                                    AssignTo = d => d.innerX
                                }
                            },
                        },
                        Transitions =
                        {
                            new Transition<(int x, (int x, int y) innerX)>
                            {
                                Message = "done.invoke.*",
                                Target = "alldone"
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
                                new Assign<(int x, int y)> { To = d => d.x, ValueFunction = d => d.x * 2 }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, int y)> { Target = "alldone" }
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

            data = await context.RunAsync(data);

            Assert.AreEqual(5, data.x);

            Assert.AreEqual(10, data.innerX.x);
        }
    }
}
