using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.DurableFunction.Client;

namespace StateChartsDotNet.DurableFunction.Host
{
    public class StateMachineOrchestration
    {
        private readonly IConfiguration _config;
        private readonly IStateMachineFactory _factory;
        private readonly ILogger<StateMachineOrchestration> _logger;

        public StateMachineOrchestration(IConfiguration config,
                                         IStateMachineFactory factory,
                                         ILogger<StateMachineOrchestration> logger)
        {
            _config = config;
            _factory = factory;
            _logger = logger;
        }

        [FunctionName("statemachine-orchestration")]
        public async Task<object> RunOrchestrator(
                [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            Debug.Assert(context != null);

            var logger = context.CreateReplaySafeLogger(_logger);

            var payload = context.GetInput<StateMachineRequestPayload>();

            Debug.Assert(payload != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(payload.StateMachineIdentifier));

            Func<string, IStateChartMetadata> lookupDefinition = identifier =>
            {
                _factory.TryResolveIdentifier(identifier, out IStateChartMetadata metadata);
                return metadata;
            };

            var stateMachineDefinition = lookupDefinition(payload.StateMachineIdentifier);

            if (stateMachineDefinition == null)
            {
                throw new InvalidOperationException($"Unable to resolve definition for state machine id: {payload.StateMachineIdentifier}");
            }

            var executionContext = new StateMachineContext(stateMachineDefinition,
                                                           context,
                                                           payload.Input,
                                                           payload.IsChildStateMachine,
                                                           payload.DebugInfo,
                                                           _config,
                                                           lookupDefinition,
                                                           logger);

            logger.LogInformation("Begin state machine execution");

            try
            {
                var interpreter = new Interpreter();

                await interpreter.RunAsync(executionContext);

                return executionContext.GetData();
            }
            finally
            {
                logger.LogInformation("End state machine execution");
            }
        }
    }
}