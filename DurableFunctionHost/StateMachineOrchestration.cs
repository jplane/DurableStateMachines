using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StateChartsDotNet;
using StateChartsDotNet.Metadata.Json.States;

namespace DurableFunctionHost
{
    public class StateMachineOrchestration
    {
        private readonly IConfiguration _config;
        private readonly ILogger<StateMachineOrchestration> _logger;

        public StateMachineOrchestration(IConfiguration config, ILogger<StateMachineOrchestration> logger)
        {
            _config = config;
            _logger = logger;
        }

        [FunctionName("statemachine-orchestration")]
        public async Task<Dictionary<string, object>> RunOrchestrator(
                [OrchestrationTrigger] IDurableOrchestrationContext context,
                [DurableClient(TaskHub = "%TASK_HUB_NAME%")] IDurableOrchestrationClient client)
        {
            Debug.Assert(context != null);
            Debug.Assert(client != null);

            var logger = context.CreateReplaySafeLogger(_logger);

            var input = context.GetInput<(Dictionary<string, object>, JObject)>();

            var data = input.Item1;

            Debug.Assert(data != null);

            var metadata = new StateChart(input.Item2);

            var executionContext = new StateMachineContext(metadata, client, context, data, logger);

            logger.LogInformation("Begin state machine execution");

            try
            {
                var interpreter = new Interpreter();

                await interpreter.RunAsync(executionContext);

                return executionContext.ResultData;
            }
            finally
            {
                logger.LogInformation("End state machine execution");
            }
        }
    }
}