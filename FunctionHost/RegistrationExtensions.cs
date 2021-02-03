using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System.Reflection;

namespace DSM.FunctionHost
{
    public static class RegistrationExtensions
    {
        public static void RegisterStateMachineDefinitionAssemblies(this IFunctionsHostBuilder builder, params Assembly[] assemblies)
        {
            DefinitionResolver.Assemblies = assemblies;     // still seems not quite ideal...
        }
    }
}
