using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using DSM.Common;
using System.Threading.Tasks;
using DSM.Common.Messages;
using System;

namespace DSM.FunctionClient
{
    public static class StateMachineExtensions
    {
        public const string StateMachineWithNameEndpoint = "statemachine-name";

        /// <summary>
        /// Start a new durable state machine orchestration.
        /// </summary>
        /// <param name="client"><see cref="IDurableClient"/> instance, usually obtained via dependency injection.</param>
        /// <param name="stateMachineId">Unique identifier for a state machine definition registered with the target Durable Functions app.</param>
        /// <param name="observer">If specified, is used to observe state machine behavior during execution.</param>
        /// <returns>The orchestration instance id for the newly started state machine.</returns>
        public static Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                             string stateMachineId,
                                                             StateMachineObserver observer = null)
        {
            return StartNewStateMachineAsync(client, stateMachineId, null, observer);
        }

        /// <summary>
        /// Start a new durable state machine orchestration.
        /// </summary>
        /// <param name="client"><see cref="IDurableClient"/> instance, usually obtained via dependency injection.</param>
        /// <param name="stateMachineId">Unique identifier for a state machine definition registered with the target Durable Functions app.</param>
        /// <param name="input">Initial execution state for the state machine instance.</param>
        /// <param name="observer">If specified, is used to observe state machine behavior during execution.</param>
        /// <returns>The orchestration instance id for the newly started state machine.</returns>
        public static async Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                                   string stateMachineId,
                                                                   object input,
                                                                   StateMachineObserver observer = null)
        {
            client.CheckArgNull(nameof(client));
            stateMachineId.CheckArgNull(nameof(stateMachineId));

            string instanceId;

            if (observer != null)
            {
                await observer.StartAsync();
                instanceId = observer.InstanceId;
            }
            else
            {
                 instanceId = Guid.NewGuid().ToString("N");
            }

            var payload = new StateMachinePayload
            {
                Observables = observer.Instructions,
                StateMachineIdentifier = stateMachineId,
                Input = input
            };

            await client.StartNewAsync(StateMachineWithNameEndpoint, instanceId, payload);

            return instanceId;
        }

        /// <summary>
        /// Raise an event of type <see cref="ExternalMessage"/> to a running state machine instance.
        /// </summary>
        /// <param name="client"><see cref="IDurableClient"/> instance, usually obtained via dependency injection.</param>
        /// <param name="instanceId">Instance id of the state machine, obtained from <see cref="StateMachineExtensions.StartNewStateMachineAsync"/></param>
        /// <param name="message">The message to send to the state machine.</param>
        public static Task SendStateMachineMessageAsync(this IDurableClient client, string instanceId, ExternalMessage message)
        {
            client.CheckArgNull(nameof(client));
            instanceId.CheckArgNull(nameof(instanceId));
            message.CheckArgNull(nameof(message));

            return client.RaiseEventAsync(instanceId, "state-machine-event", message);
        }

        /// <summary>
        /// If the state machine executed successfully, deserializes the output to an instance of <typeparamref name="TOutput"/>.
        ///  Otherwise it returns default(<typeparamref name="TOutput"/>)
        /// </summary>
        /// <typeparam name="TOutput">The execution state type for the state machine.</typeparam>
        /// <param name="status">The status of the state machine instance obtained from <see cref="StateMachineExtensions.WaitForStateMachineCompletionAsync"/></param>
        /// <returns>The deserialized output.</returns>
        public static TOutput ToOutput<TOutput>(this DurableOrchestrationStatus status)
        {
            status.CheckArgNull(nameof(status));

            if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                return status.Output.ToObject<TOutput>();
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Polls the Durable Functions app for completion status of the state machine instance.
        /// </summary>
        /// <param name="client"><see cref="IDurableClient"/> instance, usually obtained via dependency injection.</param>
        /// <param name="instanceId">Instance id of the state machine, obtained from <see cref="StateMachineExtensions.StartNewStateMachineAsync"/></param>
        /// <param name="pollingIntervalInMs">Polling interval in milliseconds. Defaults to 1000.</param>
        /// <returns>The <see cref="DurableOrchestrationStatus"/> for the state machine instance, once it completes.</returns>
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
