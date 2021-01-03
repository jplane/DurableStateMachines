using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace StateChartsDotNet.Web
{
    public class Startup
    {
        public Startup()
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Debug.Assert(app != null);
            Debug.Assert(env != null);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Debug.Assert(services != null);

            services.AddSingleton<IOrchestrationManagerHostedService, OrchestrationManagerHostedService>();

            services.AddHostedService(
                provider => (OrchestrationManagerHostedService) provider.GetService<IOrchestrationManagerHostedService>());

            services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, new StateChartInputFormatter());
                options.InputFormatters.Insert(1, new ExternalMessageInputFormatter());
                options.InputFormatters.Insert(2, new DictionaryInputFormatter());
                options.InputFormatters.Insert(3, new RegisterAndStartPayloadInputFormatter());
            });
        }
    }
}
