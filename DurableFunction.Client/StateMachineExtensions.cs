using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunction.Client
{
    public static class StateMachineExtensions
    {
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

            var payload = new StateMachineRequestPayload
            {
                StateMachineIdentifier = stateMachineId,
                Input = input,
                DebugInfo = debugInfo
            };

            return client.StartNewAsync("statemachine-orchestration", payload);
        }
    }
}
