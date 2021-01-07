using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace StateChartsDotNet.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           webBuilder.UseStartup<Startup>();

                           var port_config = webBuilder.GetSetting("FUNCTIONS_CUSTOMHANDLER_PORT");

                           if (int.TryParse(port_config, out int port))
                           {
                               webBuilder.UseUrls($"http://*:{port}");
                           }
                       });
        }
    }
}
