using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Metadata.States;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunction.Client
{
    public static class StateMachineExtensions
    {
        public static Task<string> StartNewStateMachineAsync<TData>(this IDurableClient client,
                                                                    string stateMachineId,
                                                                    DebuggerInfo debugInfo = null)
        {
            return StartNewStateMachineAsync<TData>(client, stateMachineId, default, debugInfo);
        }

        public static Task<string> StartNewStateMachineAsync<TData>(this IDurableClient client,
                                                                    string stateMachineId,
                                                                    TData arguments,
                                                                    DebuggerInfo debugInfo = null)
        {
            client.CheckArgNull(nameof(client));
            stateMachineId.CheckArgNull(nameof(stateMachineId));

            var payload = new StateMachineRequestPayload<TData>
            {
                StateMachineIdentifier = stateMachineId,
                Arguments = arguments,
                DebugInfo = debugInfo
            };

            return client.StartNewAsync("statemachine-orchestration", payload);
        }
    }
}
