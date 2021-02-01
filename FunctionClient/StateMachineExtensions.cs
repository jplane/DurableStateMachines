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

        public static TOutput ToOutput<TOutput>(this DurableOrchestrationStatus status)
        {
            if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                return status.Output.ToObject<TOutput>();
            }
            else
            {
                return default;
            }
        }

        public static async Task<DurableOrchestrationStatus> WaitForStateMachineCompletionAsync(
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

            return status;
        }
    }
}
