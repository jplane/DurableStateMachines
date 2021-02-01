using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using DSM.Common;
using DSM.Common.Debugger;
using System;
using System.Threading.Tasks;

namespace DSM.FunctionClient
{
    public static class StateMachineExtensions
    {
        public const string StateMachineWithNameEndpoint = "statemachine-name";

        public static Task<string> StartNewStateMachineAsync(this IDurableClient client, string stateMachineId)
        {
            return StartNewStateMachineAsync(client, stateMachineId, null);
        }

        public static Task<string> StartNewStateMachineAsync(this IDurableClient client, string stateMachineId, object input)
        {
            client.CheckArgNull(nameof(client));
            stateMachineId.CheckArgNull(nameof(stateMachineId));

            var payload = new StateMachinePayload
            {
                StateMachineIdentifier = stateMachineId,
                Input = input
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
