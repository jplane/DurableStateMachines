using DurableTask.Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateChartsDotNet.DurableTask;
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
    }
}
