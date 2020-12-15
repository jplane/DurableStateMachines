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

        public async Task RunAsync(ExecutionContext context, TimeSpan timeout, CancellationToken cancelToken)
        {
            context.CheckArgNull(nameof(context));

            var instanceId = $"{context.Metadata.UniqueId}.{Guid.NewGuid():N}";

            context["_invokeId"] = instanceId;

            var orchestrationManager = new DurableOrchestrationManager(_orchestrationService, timeout, cancelToken, context.Logger);

            context.SendMessageHandler = msg => orchestrationManager.SendMessageAsync(instanceId, msg);

            try
            {
                await orchestrationManager.StartAsync();

                try
                {
                    await orchestrationManager.StartOrchestrationAsync(instanceId, context);

                    var output = await orchestrationManager.WaitForCompletionAsync(instanceId);

                    Debug.Assert(output != null);

                    context.Data = new Dictionary<string, object>(output);
                }
                catch (TimeoutException)
                {
                    // cancellation token fired, we want to eat this one here
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
