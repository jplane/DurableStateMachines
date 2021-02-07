using Microsoft.VisualStudio.TestTools.UnitTesting;
using DSM.Metadata.States;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DSM.Tests
{
    [TestClass]
    public partial class SerializationActionTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task JsonSerialization(ScaffoldFactoryDelegate factory, string _)
        {
            var json = @"{
                           'id': 'test',
                           'states': [
                             {
                               'id': 'loop',
                               'type': 'atomic',
                               'onentry': {
                                 'actions': [
                                   {
                                     'type': 'foreach',
                                     'currentitemlocation': 'arrayItem',
                                     'valueexpression': 'items',
                                     'actions': [
                                       {
                                         'type': 'assign',
                                         'target': 'sum',
                                         'valueexpression': 'sum + arrayItem'
                                       },
                                       {
                                         'type': 'log',
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
                                     'type': 'log',
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
    }
}
