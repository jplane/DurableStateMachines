using DurableTask.Core;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    public class Interpreter
    {
        private readonly IOrchestrationService _orchestrationService;

        public Interpreter(IOrchestrationService orchestrationService)
        {
            orchestrationService.CheckArgNull(nameof(orchestrationService));

            if (! (orchestrationService is IOrchestrationServiceClient))
            {
                throw new ArgumentException("Expecting orchestration service to implement both client and service interfaces.");
            }

            _orchestrationService = orchestrationService;
        }

        public async Task RunAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var service = new DurableStateChartService(_orchestrationService, context);

            await service.StartAsync();

            var client = new DurableStateChartClient((IOrchestrationServiceClient) _orchestrationService, context);

            await client.InitAsync();

            await client.WaitForCompletionAsync(TimeSpan.FromSeconds(60));

            await service.StopAsync();
        }
    }
}
