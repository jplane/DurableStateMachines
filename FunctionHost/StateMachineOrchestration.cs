using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using DSM.Common.Observability;
using DSM.Common.Model.States;
using DSM.Engine;
using DSM.FunctionClient;

namespace DSM.FunctionHost
{
    public static class StateMachineOrchestration
    {
        public static async Task<object> RunStateMachineWithNameAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            Debug.Assert(context != null);

            logger = context.CreateReplaySafeLogger(logger);

            var payload = context.GetInput<StateMachinePayload>();

            Debug.Assert(payload != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(payload.StateMachineIdentifier));

            var resolver = new DefinitionResolver();

            var stateMachineDefinition = resolver.Resolve(payload.StateMachineIdentifier);

            if (stateMachineDefinition == null)
            {
                throw new InvalidOperationException($"Unable to resolve definition for state machine id: {payload.StateMachineIdentifier}");
            }

            return await RunAsync(context,
                                  stateMachineDefinition,
                                  payload.DeserializeInput(stateMachineDefinition),
                                  payload.ParentInstanceStack,
                                  payload.Observables,
                                  resolver.Resolve,
                                  logger);
        }

        public static async Task<IDictionary<string, object>> RunStateMachineWithDefinitionAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            Debug.Assert(context != null);

            logger = context.CreateReplaySafeLogger(logger);

            var payload = context.GetInput<StateMachineDefinitionPayload>();

            Debug.Assert(payload != null);
            Debug.Assert(payload.Definition != null);

            var input = payload.Input ?? new Dictionary<string, object>();

            var result = await RunAsync(context,
                                        payload.Definition,
                                        input,
                                        payload.ParentInstanceStack,
                                        payload.Observables,
                                        null,
                                        logger);

            Debug.Assert(result != null);

            return (IDictionary<string, object>) result;
        }

        private static async Task<object> RunAsync(IDurableOrchestrationContext context,
                                                   IStateMachineMetadata stateMachineDefinition,
                                                   object input,
                                                   string[] parentInstanceStack,
                                                   Instruction[] observableInstructions,
                                                   Func<string, IStateMachineMetadata> resolver,
                                                   ILogger logger)
        {
            Debug.Assert(context != null);
            Debug.Assert(stateMachineDefinition != null);

            var executionContext = new StateMachineContext(stateMachineDefinition,
                                                           context,
                                                           input,
                                                           parentInstanceStack,
                                                           observableInstructions,
                                                           Startup.Configuration,
                                                           resolver,
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