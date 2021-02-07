using Microsoft.VisualStudio.TestTools.UnitTesting;
using DSM.Metadata.Actions;
using DSM.Metadata.States;
using System.Threading.Tasks;

namespace DSM.Tests
{
    [TestClass]
    public partial class IfActionTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task If(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine<(string x, string y, string z)>
            {
                Id = "only-if",
                States =
                {
                    new AtomicState<(string x, string y, string z)>
                    {
                        Id = "main",
                        OnEntry = new OnEntryExit<(string x, string y, string z)>
                        {
                            Actions =
                            {
                                new If<(string x, string y, string z)>
                                {
                                    ConditionFunction = d => string.Compare(d.x, d.y) == 0,
                                    Actions =
                                    {
                                        new Assign<(string x, string y, string z)> { To = d => d.z, ValueFunction = _ => "IF" }
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(string x, string y, string z)>
                            {
                                Target = "done"
                            }
                        }
                    },
                    new FinalState<(string x, string y, string z)>
                    {
                        Id = "done"
                    }
                }
            };

            var data = ("hi", "hi", string.Empty);

            var tuple = factory(machine, null, null);

            var context = tuple.Item1;

            data = await context.RunAsync(data);    // notice we need the return value here... data is ValueTuple (value type)

            Assert.AreEqual("IF", data.Item3);
        }

        [TestMethod]
        [TestScaffold]
        public async Task ElseIf(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine<(string x, string y, string z)>
            {
                Id = "else-if",
                States =
                {
                    new AtomicState<(string x, string y, string z)>
                    {
                        Id = "main",
                        OnEntry = new OnEntryExit<(string x, string y, string z)>
                        {
                            Actions =
                            {
                                new If<(string x, string y, string z)>
                                {
                                    ConditionFunction = d => string.Compare(d.x, d.y) == 0,
                                    Actions =
                                    {
                                        new Assign<(string x, string y, string z)> { To = d => d.z, ValueFunction = _ => "IF" }
                                    },
                                    ElseIfs =
                                    {
                                        new ElseIf<(string x, string y, string z)>
                                        {
                                            ConditionFunction = d => string.Compare(d.x, d.y) < 0,
                                            Actions =
                                            {
                                                new Assign<(string x, string y, string z)> { To = d => d.z, ValueFunction = _ => "ELSEIF" }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(string x, string y, string z)>
                            {
                                Target = "done"
                            }
                        }
                    },
                    new FinalState<(string x, string y, string z)>
                    {
                        Id = "done"
                    }
                }
            };

            var data = ("abc", "xyz", string.Empty);

            var tuple = factory(machine, null, null);

            var context = tuple.Item1;

            data = await context.RunAsync(data);    // notice we need the return value here... data is ValueTuple (value type)

            Assert.AreEqual("ELSEIF", data.Item3);
        }

        [TestMethod]
        [TestScaffold]
        public async Task Else(ScaffoldFactoryDelegate factory, string _)
        {
            var machine = new StateMachine<(string x, string y, string z)>
            {
                Id = "else-if",
                States =
                {
                    new AtomicState<(string x, string y, string z)>
                    {
                        Id = "main",
                        OnEntry = new OnEntryExit<(string x, string y, string z)>
                        {
                            Actions =
                            {
                                new If<(string x, string y, string z)>
                                {
                                    ConditionFunction = d => string.Compare(d.x, d.y) == 0,
                                    Actions =
                                    {
                                        new Assign<(string x, string y, string z)> { To = d => d.z, ValueFunction = _ => "IF" }
                                    },
                                    ElseIfs =
                                    {
                                        new ElseIf<(string x, string y, string z)>
                                        {
                                            ConditionFunction = d => string.Compare(d.x, d.y) < 0,
                                            Actions =
                                            {
                                                new Assign<(string x, string y, string z)> { To = d => d.z, ValueFunction = _ => "ELSEIF" }
                                            }
                                        }
                                    },
                                    Else = new Else<(string x, string y, string z)>
                                    {
                                        Actions =
                                        {
                                            new Assign<(string x, string y, string z)> { To = d => d.z, ValueFunction = _ => "ELSE" }
                                        }
                                    }
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(string x, string y, string z)>
                            {
                                Target = "done"
                            }
                        }
                    },
                    new FinalState<(string x, string y, string z)>
                    {
                        Id = "done"
                    }
                }
            };

            var data = ("xyz", "abc", string.Empty);

            var tuple = factory(machine, null, null);

            var context = tuple.Item1;

            data = await context.RunAsync(data);    // notice we need the return value here... data is ValueTuple (value type)

            Assert.AreEqual("ELSE", data.Item3);
        }
    }
}
