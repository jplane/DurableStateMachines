using DurableTask.Core;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

        public async Task RunAsync(ExecutionContext context, CancellationToken cancelToken)
        {
            context.CheckArgNull(nameof(context));

            var timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(1);  // TODO: expose externally

            var instanceId = context.Metadata.Id;

            var orchestrationManager = new DurableOrchestrationManager(_orchestrationService, context.Logger);

            context.SendMessageHandler = msg => orchestrationManager.SendMessageAsync(instanceId, msg);

            try
            {
                await orchestrationManager.StartAsync(context, cancelToken);

                try
                {
                    await orchestrationManager.StartOrchestrationAsync();

                    var output = await orchestrationManager.WaitForCompletionAsync(instanceId, timeout, cancelToken);

                    Debug.Assert(output != null);

                    context.Data = new Dictionary<string, object>(output);
                }
                finally
                {
                    await orchestrationManager.StopAsync();
                }
            }
            finally
            {
                context.SendMessageHandler = null;
            }
        }
    }
}
