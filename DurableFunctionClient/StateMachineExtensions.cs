using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunctionClient
{
    public static class StateMachineExtensions
    {
        public static Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                             IStateChartMetadata definition,
                                                             DebuggerInfo debugInfo = null)
        {
            return StartNewStateMachineAsync(client, definition, null, debugInfo);
        }

        public static Task<string> StartNewStateMachineAsync(this IDurableClient client,
                                                             IStateChartMetadata definition,
                                                             IDictionary<string, object> arguments,
                                                             DebuggerInfo debugInfo = null)
        {
            client.CheckArgNull(nameof(client));
            definition.CheckArgNull(nameof(definition));

            var format = StateMachineDefinitionFormat.Fluent;

            if (definition is Metadata.Json.States.StateChart)
            {
                format = StateMachineDefinitionFormat.Json;
            }

            var payload = new StateMachineRequestPayload
            {
                StateMachineDefinition = definition.ToJson(),
                Format = format,
                Arguments = arguments == null ? null : new Dictionary<string, object>(arguments),
                DebugInfo = debugInfo
            };

            return client.StartNewAsync("statemachine-orchestration", payload);
        }
    }
}
