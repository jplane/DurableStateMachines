using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Json.States;
using System;
using System.Collections.Generic;

namespace DurableFunctionHost
{
    internal class StateMachineRequestPayload
    {
        public static StateMachineRequestPayload Deserialize(IDurableOrchestrationContext context)
        {
            context.CheckArgNull(nameof(context));

            var json = context.GetInput<JObject>();

            var argumentsJson = json.Property("args")?.Value.Value<JObject>();

            var arguments = argumentsJson == null ?
                                new Dictionary<string, object>() :
                                argumentsJson.ToObject<Dictionary<string, object>>();

            var definitionJson = json.Property("statemachine")?.Value.Value<JObject>();

            if (definitionJson == null)
            {
                throw new InvalidOperationException("Orchestration input must include state machine definition.");
            }

            var metadata = new StateChart(definitionJson);

            return new StateMachineRequestPayload(arguments, metadata);
        }

        private StateMachineRequestPayload(Dictionary<string, object> arguments, IStateChartMetadata definition)
        {
            this.Arguments = arguments;
            this.StateMachineDefinition = definition;
        }

        public IReadOnlyDictionary<string, object> Arguments { get; }

        public IStateChartMetadata StateMachineDefinition { get; }
    }
}
