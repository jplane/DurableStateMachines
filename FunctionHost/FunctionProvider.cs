using Microsoft.Azure.WebJobs.Script.Description;
using Newtonsoft.Json.Linq;
using DSM.FunctionClient;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSM.FunctionHost
{
    internal class FunctionProvider : IFunctionProvider
    {
        public const string StateMachineWithDefinitionEndpoint = "statemachine-definition";

        public const string StateMachineDebuggerBreakEndpoint = "statemachine-debugger-break";

        static readonly string CurrentAssemblyFile = $"assembly:{Assembly.GetExecutingAssembly().FullName}";

        static readonly string SimpleAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        ImmutableDictionary<string, ImmutableArray<string>> IFunctionProvider.FunctionErrors =>
            new Dictionary<string, ImmutableArray<string>>().ToImmutableDictionary();

        Task<ImmutableArray<FunctionMetadata>> IFunctionProvider.GetFunctionMetadataAsync() =>
            Task.FromResult(GetFunctions().ToImmutableArray());

        static IEnumerable<FunctionMetadata> GetFunctions()
        {
            var metadata = new List<FunctionMetadata>();

            metadata.Add(new FunctionMetadata
            {
                Name = StateMachineExtensions.StateMachineWithNameEndpoint,
                Bindings =
                {
                    BindingMetadata.Create(new JObject(
                                                new JProperty("type", "orchestrationTrigger"),
                                                new JProperty("name", "context")))
                },
                ScriptFile = CurrentAssemblyFile,
                EntryPoint = $"{SimpleAssemblyName}.{nameof(StateMachineOrchestration)}.{nameof(StateMachineOrchestration.RunStateMachineWithNameAsync)}",
                Language = "DotNetAssembly"
            });

            metadata.Add(new FunctionMetadata
            {
                Name = StateMachineWithDefinitionEndpoint,
                Bindings =
                {
                    BindingMetadata.Create(new JObject(
                                                new JProperty("type", "orchestrationTrigger"),
                                                new JProperty("name", "context")))
                },
                ScriptFile = CurrentAssemblyFile,
                EntryPoint = $"{SimpleAssemblyName}.{nameof(StateMachineOrchestration)}.{nameof(StateMachineOrchestration.RunStateMachineWithDefinitionAsync)}",
                Language = "DotNetAssembly"
            });

            metadata.Add(new FunctionMetadata
            {
                Name = StateMachineDebuggerBreakEndpoint,
                Bindings =
                {
                    BindingMetadata.Create(new JObject(
                                                new JProperty("type", "activityTrigger"),
                                                new JProperty("name", "context")))
                },
                ScriptFile = CurrentAssemblyFile,
                EntryPoint = $"{SimpleAssemblyName}.{nameof(ObserverActivity)}.{nameof(ObserverActivity.RunAsync)}",
                Language = "DotNetAssembly"
            });

            return metadata;
        }
    }
}
