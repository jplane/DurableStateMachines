using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Metadata.Json.States;
using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class JsonTests : TestBase
    {
        [TestMethod]
        [TestScaffold]
        public async Task Foreach(ScaffoldFactoryDelegate factory, string _)
        {
            var json = @"{
                             'name': 'test',
                             'initial': 'loop',
                             'datamodel': [
                                 { 'id': 'items', 'expr': 'new [] { 1, 2, 3, 4, 5 }' },
                                 { 'id': 'sum', 'expr': '0' }
                             ],
                             'states': [
                                 {
                                     'id': 'loop',
                                     'onentry': {
                                         'content': [
                                             {
                                                 'type': 'foreach',
                                                 'array': 'items',
                                                 'item': 'arrayItem',
                                                 'content': [
                                                     { 'type': 'assign', 'location': 'sum', 'expr': 'sum + arrayItem' },
                                                     { 'type': 'log', 'expr': '""item = "" + arrayItem' }
                                                 ]
                                             }
                                         ]
                                     },
                                     'transitions': [
                                         { 'cond': 'sum >= 15', 'target': 'done' }
                                     ]
                                 },
                                 {
                                     'id': 'done',
                                     'type': 'final',
                                     'onentry': {
                                         'content': [
                                             { 'type': 'log', 'expr': '""item = "" + arrayItem' }
                                         ]
                                     }
                                 }
                             ]
                         }";

            var machine = new StateChart(JObject.Parse(json));

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAndWaitForCompletionAsync();

            Assert.AreEqual(15, Convert.ToInt32(context.Data["sum"]));
        }

        [TestMethod]
        [TestScaffold]
        public async Task HttpPost(ScaffoldFactoryDelegate factory, string _)
        {
            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync("http://localhost:4444/"));

            var json = @"{
                             'name': 'test',
                             'states': [
                                 {
                                     'id': 'state1',
                                     'onentry': {
                                         'content': [
                                             {
                                                 'type': 'http-post',
                                                 'url': 'http://localhost:4444/',
                                                 'body': {
                                                     'value': 5
                                                 }
                                             }
                                         ]
                                     },
                                     'transitions': [
                                         { 'target': 'alldone' }
                                     ]
                                 },
                                 {
                                     'id': 'alldone',
                                     'type': 'final'
                                 }
                             ]
                         }";

            var machine = new StateChart(JObject.Parse(json));

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

            var json = @"{
                             'name': 'test',
                             'states': [
                                 {
                                     'id': 'state1',
                                     'onentry': {
                                         'content': [
                                             {
                                                 'type': 'http-get',
                                                 'url': 'http://localhost:4444/',
                                                 'resultlocation': 'x'
                                             }
                                         ]
                                     },
                                     'transitions': [
                                         { 'target': 'alldone' }
                                     ]
                                 },
                                 {
                                     'id': 'alldone',
                                     'type': 'final'
                                 }
                             ]
                         }";

            var machine = new StateChart(JObject.Parse(json));

            var tuple = factory(machine, Logger);

            var context = tuple.Item1;

            var task = context.StartAndWaitForCompletionAsync();

            await Task.WhenAll(task, listenerTask);

            var jsonResult = (string) context.Data["x"];

            Assert.IsNotNull(json);

            var content = JsonConvert.DeserializeAnonymousType(jsonResult, new { value = default(int) });

            Assert.AreEqual(43, content.value);
        }

        [TestMethod]
        [TestScaffold]
        public async Task Microwave(ScaffoldFactoryDelegate factory, string _)
        {
            var json = @"{
                             'name': 'test',
                             'initial': 'off',
                             'datamodel': [
                                 { 'id': 'cook_time', 'expr': '5' },
                                 { 'id': 'door_closed', 'expr': 'true' },
                                 { 'id': 'timer', 'expr': '0' }
                             ],
                             'states': [
                                 {
                                     'id': 'off',
                                     'transitions': [
                                         { 'event': 'turn.on', 'target': 'on' }
                                     ]
                                 },
                                 {
                                     'id': 'on',
                                     'initial': 'idle',
                                     'transitions': [
                                         { 'event': 'turn.off', 'target': 'off' },
                                         { 'cond': 'timer >= cook_time', 'target': 'off' }
                                     ],
                                     'states': [
                                         {
                                             'id': 'idle',
                                             'transitions': [
                                                 { 'cond': 'door_closed', 'target': 'cooking' },
                                                 {
                                                     'event': 'door.close',
                                                     'target': 'cooking',
                                                     'content': [
                                                        { 'type': 'assign', 'location': 'door_closed', 'expr': 'true' }
                                                     ]
                                                 }
                                             ]
                                         },
                                         {
                                             'id': 'cooking',
                                             'transitions': [
                                                 {
                                                     'event': 'door.open',
                                                     'target': 'idle',
                                                     'content': [
                                                        { 'type': 'assign', 'location': 'door_closed', 'expr': 'false' }
                                                     ]
                                                 },
                                                 {
                                                     'event': 'time',
                                                     'content': [
                                                        { 'type': 'assign', 'location': 'timer', 'expr': 'timer + 1' }
                                                     ]
                                                 }
                                             ]
                                         }
                                     ]
                                 }
                             ]
                         }";

            var machine = new StateChart(JObject.Parse(json));

            var tuple = factory(machine, null);

            var context = tuple.Item1;

            await context.StartAsync();

            await Task.Delay(1000);

            await context.SendMessageAsync("turn.on");

            for (var i = 0; i < 5; i++)
            {
                await Task.Delay(1000);
                await context.SendMessageAsync("time");
            }

            await Task.Delay(1000);

            await context.SendStopMessageAsync();

            await context.WaitForCompletionAsync();

            Assert.AreEqual(5, Convert.ToInt32(context.Data["timer"]));
        }

        [TestMethod]
        [TestScaffold]
        public async Task SimpleParentChild(ScaffoldFactoryDelegate factory, string _)
        {
            var json = @"{
                             'name': 'outer',
                             'states': [
                                 {
                                     'id': 'outerState1',
                                     'invokes': [
                                         {
                                             'params': [
                                                 { 'name': 'x', 'location': 'x' }
                                             ],
                                             'content': {
                                                 'name': 'inner',
                                                 'states': [
                                                     {
                                                         'id': 'innerState1',
                                                         'onentry': {
                                                             'content': [
                                                                 { 'type': 'assign', 'location': 'x', 'expr': 'x * 2' }
                                                             ]
                                                         },
                                                         'transitions': [
                                                             { 'target': 'alldone' }
                                                         ]
                                                     },
                                                     {
                                                         'id': 'alldone',
                                                         'type': 'final',
                                                         'donedata': {
                                                             'params': [
                                                                 { 'name': 'innerX', 'location': 'x' }
                                                             ]
                                                         }
                                                     }
                                                 ]
                                             },
                                             'finalize': [
                                                 { 'type': 'assign', 'location': 'innerX', 'expr': '_event.Parameters[""innerX""]' }
                                             ]
                                         }
                                     ],
                                     'transitions': [
                                         { 'event': 'done.invoke.*', 'target': 'alldone' }
                                     ]
                                 },
                                 {
                                     'id': 'alldone',
                                     'type': 'final'
                                 }
                             ]
                         }";

            var machine = new StateChart(JObject.Parse(json));

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
