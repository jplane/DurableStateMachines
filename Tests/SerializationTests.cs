using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Common.ExpressionTrees;
using StateChartsDotNet.Metadata.Execution;
using StateChartsDotNet.Metadata.States;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class SerializationTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public void States(ScaffoldFactoryDelegate _, string __)
        {
            var machine = new StateMachine
            {
                Id = "test",
                States =
                {
                    new AtomicState
                    {
                        Id = "s1",
                        Transitions =
                        {
                            new Transition
                            {
                                Targets = { "done" }
                            }
                        }
                    },
                    new FinalState
                    {
                        Id = "done"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(machine);

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var machine2 = JsonConvert.DeserializeObject<StateMachine>(json);

            Assert.IsNotNull(machine2);

            Assert.AreEqual(machine.Id, machine2.Id);
            Assert.AreEqual(2, machine2.States.Count);
            Assert.IsTrue(machine2.States[0] is AtomicState);
            Assert.AreEqual("s1", machine2.States[0].Id);
            Assert.IsTrue(machine2.States[1] is FinalState);
            Assert.AreEqual("done", machine2.States[1].Id);
        }

        [TestMethod]
        [TestScaffold]
        public void Actions(ScaffoldFactoryDelegate _, string __)
        {
            var machine = new StateMachine
            {
                Id = "test",
                States =
                {
                    new AtomicState
                    {
                        Id = "s1",
                        OnEntry = new OnEntryExit
                        {
                            Actions =
                            {
                                new Assign { Location = "y", Value = 45 },
                                new Script { Expression = "z = y * y" }
                            }
                        }
                    },
                    new FinalState
                    {
                        Id = "done"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(machine);

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var machine2 = JsonConvert.DeserializeObject<StateMachine>(json);

            Assert.IsNotNull(machine2);

            Assert.AreEqual(machine.Id, machine2.Id);
            Assert.AreEqual(2, machine2.States.Count);
            Assert.IsTrue(machine2.States[0] is AtomicState);
            Assert.AreEqual("s1", machine2.States[0].Id);

            var onEntry = (machine2.States[0] as AtomicState).OnEntry;

            Assert.IsNotNull(onEntry);
            Assert.AreEqual(2, onEntry.Actions.Count);
            Assert.IsTrue(onEntry.Actions[0] is Assign);
            Assert.IsTrue(onEntry.Actions[1] is Script);

            Assert.IsTrue(machine2.States[1] is FinalState);
            Assert.AreEqual("done", machine2.States[1].Id);
        }

        [TestMethod]
        [TestScaffold]
        public void ExpressionTree(ScaffoldFactoryDelegate _, string __)
        {
            var dict = new Dictionary<string, object>
            {
                { "x", 4 },
                { "y", 5 }
            };

            Expression<Func<IDictionary<string, object>, object>> func = data => ((int) data["x"]) + ((int) data["y"]);

            var adder = func.Compile();

            Assert.AreEqual(9, adder(dict));

            var settings = new JsonSerializerSettings
            {
                Converters = { new ExpressionTreeConverter() }
            };

            var json = JsonConvert.SerializeObject(func, settings);

            Assert.IsFalse(string.IsNullOrWhiteSpace(json));

            var func2 = JsonConvert.DeserializeObject<Expression<Func<IDictionary<string, object>, object>>>(json, settings);

            Assert.IsNotNull(func2);

            var adder2 = func2.Compile();

            Assert.AreEqual(9, adder2(dict));
        }
    }
}
