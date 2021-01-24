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
        public static Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                             StateMachine definition,
                                                             DebuggerInfo debugInfo = null)
        {
            return StartNewStateMachineAsync(client, definition, null, debugInfo);
        }

        public static Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                             StateMachine definition,
                                                             IDictionary<string, object> arguments,
                                                             DebuggerInfo debugInfo = null)
        {
            client.CheckArgNull(nameof(client));
            definition.CheckArgNull(nameof(definition));

            var payload = new StateMachineRequestPayload
            {
                StateMachineDefinition = definition,
                Arguments = arguments?.ToDictionary(p => p.Key, p => p.Value) ?? new Dictionary<string, object>(),
                DebugInfo = debugInfo
            };

            return client.StartNewAsync("statemachine-orchestration", payload);
        }
    }
}
