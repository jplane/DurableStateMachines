using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using DSM.Common;
using DSM.Metadata.Actions;
using DSM.Metadata.States;
using System.Threading.Tasks;

namespace DSM.Tests
{
    [TestClass]
    public partial class RaiseActionTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task ConditionalTransition(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine<(int x, string y)>
            {
                Id = "raise",
                States =
                {
                    new AtomicState<(int x, string y)>
                    {
                        Id = "start",
                        OnEntry = new OnEntryExit<(int x, string y)>
                        {
                            Actions =
                            {
                                new If<(int x, string y)>
                                {
                                    ConditionFunction = d => d.x > 10,
                                    Actions =
                                    {
                                        new Raise<(int x, string y)> { Message = "greater" }
                                    },
                                    ElseIfs =
                                    {
                                        new ElseIf<(int x, string y)>
                                        {
                                            ConditionFunction = d => d.x < 10,
                                            Actions =
                                            {
                                                new Raise<(int x, string y)> { Message = "lesser" }
                                            }
                                        }
                                    },
                                    Else = new Else<(int x, string y)>
                                    {
                                        Actions =
                                        {
                                            new Raise<(int x, string y)> { Message = "equal" }
                                        }
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, string y)>
                            {
                                Message = "greater",
                                Target = "greater-state"
                            },
                            new Transition<(int x, string y)>
                            {
                                Message = "lesser",
                                Target = "lesser-state"
                            },
                            new Transition<(int x, string y)>
                            {
                                Message = "equal",
                                Target = "equal-state"
                            }
                        }
                    },
                    new AtomicState<(int x, string y)>
                    {
                        Id = "greater-state",
                        OnEntry = new OnEntryExit<(int x, string y)>
                        {
                            Actions =
                            {
                                new Assign<(int x, string y)> { To = d => d.y, Value = "is-greater" }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, string y)>
                            {
                                Target = "done"
                            }
                        }
                    },
                    new AtomicState<(int x, string y)>
                    {
                        Id = "lesser-state",
                        OnEntry = new OnEntryExit<(int x, string y)>
                        {
                            Actions =
                            {
                                new Assign<(int x, string y)> { To = d => d.y, Value = "is-lesser" }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, string y)>
                            {
                                Target = "done"
                            }
                        }
                    },
                    new AtomicState<(int x, string y)>
                    {
                        Id = "equal-state",
                        OnEntry = new OnEntryExit<(int x, string y)>
                        {
                            Actions =
                            {
                                new Assign<(int x, string y)> { To = d => d.y, Value = "is-equal" }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, string y)>
                            {
                                Target = "done"
                            }
                        }
                    },
                    new FinalState<(int x, string y)>
                    {
                        Id = "done"
                    }
                }
            };

            var data = (5, string.Empty);

            var tuple = factory(machine, null, null);

            var context = tuple.Item1;

            data = await context.RunAsync(data);    // notice we need the return value here... data is ValueTuple (value type)

            Assert.AreEqual("is-lesser", data.Item2);

            data = (15, string.Empty);

            data = await context.RunAsync(data);

            Assert.AreEqual("is-greater", data.Item2);

            data = (10, string.Empty);

            data = await context.RunAsync(data);

            Assert.AreEqual("is-equal", data.Item2);
        }
    }
}
