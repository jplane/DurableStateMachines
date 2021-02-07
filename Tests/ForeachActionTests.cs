using Microsoft.VisualStudio.TestTools.UnitTesting;
using DSM.Metadata.Actions;
using DSM.Metadata.States;
using System.Threading.Tasks;

namespace DSM.Tests
{
    [TestClass]
    public partial class ForeachActionTests : TestBase
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
    }
}
