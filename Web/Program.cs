using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace StateChartsDotNet.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host_port = Environment.GetEnvironmentVariable("FUNCTIONS_CUSTOMHANDLER_PORT");

            if (!int.TryParse(host_port, out int port))
            {
                port = 8081;
            }

            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>()
                                                                         .UseUrls($"http://*:{port}")
                                                                         .UseShutdownTimeout(TimeSpan.FromSeconds(10)));
        }
    }
}
