using EmbedIO;
using EmbedIO.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Tests
{
    public static class InProcWebServer
    {
        public static async Task<string> RunAsync(string uri)
        {
            var completion = new TaskCompletionSource<string>();

            RequestHandlerCallback callback = async ctx =>
            {
                completion.SetResult(await ctx.GetRequestBodyAsStringAsync());
            };

            var server = new WebServer(options => options.WithUrlPrefix(uri)
                                                         .WithMode(HttpListenerMode.EmbedIO))
                                                         .WithModule(new ActionModule("/", HttpVerbs.Any, callback));

            using (server)
            {
                server.RunAsync();

                return await completion.Task;
            }
        }

        public static string TrimResult(this string s)
        {
            return new string(s.SkipWhile(c => c != '{')
                               .TakeWhile(c => c != '}')
                               .Append('}')
                               .ToArray());
        }
    }
}
