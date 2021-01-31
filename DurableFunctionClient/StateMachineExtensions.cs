using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunctionClient
{
    public static class StateMachineExtensions
    {
        public const string StateMachineWithNameEndpoint = "statemachine-name";

        public static Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                             string stateMachineId,
                                                             DebuggerInfo debugInfo = null)
        {
            return StartNewStateMachineAsync(client, stateMachineId, null, debugInfo);
        }

        public static Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                             string stateMachineId,
                                                             object input,
                                                             DebuggerInfo debugInfo = null)
        {
            client.CheckArgNull(nameof(client));
            stateMachineId.CheckArgNull(nameof(stateMachineId));

            var payload = new StateMachinePayload
            {
                StateMachineIdentifier = stateMachineId,
                Input = input,
                DebugInfo = debugInfo
            };

            return client.StartNewAsync(StateMachineWithNameEndpoint, payload);
        }

        public static async Task<(TResult, DurableOrchestrationStatus)> WaitForStateMachineCompletionAsync<TResult>(
            this IDurableClient client, string instanceId, int pollingIntervalInMs = 1000)
        {
            client.CheckArgNull(nameof(client));
            instanceId.CheckArgNull(nameof(instanceId));

            DurableOrchestrationStatus status = null;

            var done = false;

            while (!done)
            {
                await Task.Delay(pollingIntervalInMs);

                status = await client.GetStatusAsync(instanceId);

                done = status.RuntimeStatus == OrchestrationRuntimeStatus.Canceled ||
                       status.RuntimeStatus == OrchestrationRuntimeStatus.Completed ||
                       status.RuntimeStatus == OrchestrationRuntimeStatus.Failed ||
                       status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated;
            }

            TResult result = default;

            if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                result = status.Output.ToObject<TResult>();
            }

            return (result, status);
        }
    }
}
