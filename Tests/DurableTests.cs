using DurableTask.Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using StateChartsDotNet.Durable;
using StateChartsDotNet.Metadata.Fluent.Services.HttpPost;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    [TestClass]
    public class DurableTests
    {
        [TestMethod]
        public async Task SimpleTransition()
        {
            var x = 1;

            var machine = StateChart.Define("test")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .Execute(_ => x += 1)
                                            .Attach()
                                        .OnExit()
                                            .Execute(_ => x += 1)
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var emulator = new LocalOrchestrationService();

            var service = new DurableStateChartService(emulator, machine);

            await service.StartAsync();

            var client = new DurableStateChartClient(emulator, machine.Id);

            await client.InitAsync();

            await client.WaitForCompletionAsync(TimeSpan.FromSeconds(60));

            await service.StopAsync();

            Assert.AreEqual(3, x);
        }

        [TestMethod]
        public async Task SimpleParentChild()
        {
            var x = 1;

            var innerMachine = StateChart.Define("inner")
                                         .AtomicState("innerState1")
                                             .OnEntry()
                                                 .Execute(_ => x += 1)
                                                 .Attach()
                                             .OnExit()
                                                 .Execute(_ => x += 1)
                                                 .Attach()
                                             .Transition()
                                                 .Target("alldone")
                                                 .Attach()
                                             .Attach()
                                         .FinalState("alldone")
                                             .Attach();

            var machine = StateChart.Define("outer")
                                    .AtomicState("outerState1")
                                        .InvokeStateChart()
                                            .Definition(innerMachine)
                                            .Attach()
                                        .Transition()
                                            .Message("done.invoke.*")
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var emulator = new LocalOrchestrationService();

            var service = new DurableStateChartService(emulator, machine);

            await service.StartAsync();

            var client = new DurableStateChartClient(emulator, machine.Id);

            await client.InitAsync();

            await client.WaitForCompletionAsync(TimeSpan.FromSeconds(60));

            await service.StopAsync();

            Assert.AreEqual(3, x);
        }

        [TestMethod]
        public async Task Delay()
        {
            var uri = "http://localhost:4444/";

            var listenerTask = Task.Run(() => InProcWebServer.EchoAsync(uri));

            var now = DateTimeOffset.UtcNow;

            var machine = StateChart.Define("httptest")
                                    .AtomicState("state1")
                                        .OnEntry()
                                            .HttpPost()
                                                .Url(uri)
                                                .Delay(TimeSpan.FromSeconds(5))
                                                .Body(_ => new { value = DateTimeOffset.UtcNow })
                                                .Attach()
                                            .Attach()
                                        .Transition()
                                            .Target("alldone")
                                            .Attach()
                                        .Attach()
                                    .FinalState("alldone")
                                        .Attach();

            var emulator = new LocalOrchestrationService();

            var service = new DurableStateChartService(emulator, machine);

            await service.StartAsync();

            var client = new DurableStateChartClient(emulator, machine.Id);

            await client.InitAsync();

            await client.WaitForCompletionAsync(TimeSpan.FromSeconds(60));

            await service.StopAsync();

            var json = await listenerTask;

            var content = JsonConvert.DeserializeAnonymousType(json, new { value = default(DateTimeOffset) });

            Assert.IsTrue(content.value >= now + TimeSpan.FromSeconds(5));
        }
    }
}
