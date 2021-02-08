using ClassLibrary1;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using DSM.FunctionHost;

[assembly: FunctionsStartup(typeof(FunctionApp1.Startup))]

namespace FunctionApp1
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.RegisterStateMachineDefinitionAssemblies(typeof(Definitions).Assembly);
        }
    }
}
