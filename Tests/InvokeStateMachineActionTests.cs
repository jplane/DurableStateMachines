using Microsoft.VisualStudio.TestTools.UnitTesting;
using DSM.Metadata.Actions;
using DSM.Metadata.States;
using System.Threading.Tasks;

namespace DSM.Tests
{
    [TestClass]
    public partial class InvokeStateMachineActionTests : TestBase
    {
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
