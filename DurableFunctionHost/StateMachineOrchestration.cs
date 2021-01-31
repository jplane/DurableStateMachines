using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.DurableFunctionClient;

namespace StateChartsDotNet.DurableFunctionHost
{
    public static class StateMachineOrchestration
    {
        public static IConfiguration Configuration { get; set; }    // yuck

        public static async Task<object> RunStateMachineWithNameAsync(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger logger)
        {
            Debug.Assert(context != null);

            logger = context.CreateReplaySafeLogger(logger);

            var payload = context.GetInput<StateMachinePayload>();

            Debug.Assert(payload != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(payload.StateMachineIdentifier));

            var resolver = new DefinitionResolver(Configuration);

            var stateMachineDefinition = resolver.Resolve(payload.StateMachineIdentifier);

            if (stateMachineDefinition == null)
            {
                throw new InvalidOperationException($"Unable to resolve definition for state machine id: {payload.StateMachineIdentifier}");
            }

            return await RunAsync(context,
                                  stateMachineDefinition,
                                  payload.DeserializeInput(stateMachineDefinition),
                                  payload.IsChildStateMachine,
                                  payload.DebugInfo,
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

            var result = await RunAsync(context,
                                        payload.Definition,
                                        payload.Input,
                                        payload.IsChildStateMachine,
                                        payload.DebugInfo,
                                        _ => null,
                                        logger);

            Debug.Assert(result != null);

            return (IDictionary<string, object>) result;
        }

        private static async Task<object> RunAsync(IDurableOrchestrationContext context,
                                                   IStateChartMetadata stateMachineDefinition,
                                                   object input,
                                                   bool isChildStateMachine,
                                                   DebuggerInfo debugInfo,
                                                   Func<string, IStateChartMetadata> resolver,
                                                   ILogger logger)
        {
            Debug.Assert(context != null);
            Debug.Assert(stateMachineDefinition != null);

            var executionContext = new StateMachineContext(stateMachineDefinition,
                                                           context,
                                                           input,
                                                           isChildStateMachine,
                                                           debugInfo,
                                                           Configuration,
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