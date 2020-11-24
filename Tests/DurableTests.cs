﻿using DurableTask.Emulator;
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

            var service = new DurableStateChartService(machine, emulator);

            await service.StartAsync();

            var client = new DurableStateChartClient(emulator);

            await client.StartAsync();

            await client.WaitForCompletionAsync(TimeSpan.FromSeconds(60));

            await service.StopAsync();

            Assert.AreEqual(3, x);
        }
    }
}
