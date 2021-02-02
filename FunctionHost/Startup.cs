using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Script.Description;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(DSM.FunctionHost.Startup))]

namespace DSM.FunctionHost
{
    public class Startup : FunctionsStartup
    {
        internal static IConfiguration Configuration { get; private set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IFunctionProvider, FunctionProvider>();

            Startup.Configuration = builder.GetContext().Configuration;   // yuck
        }
    }
}
