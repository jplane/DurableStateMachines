using EmbedIO;
using EmbedIO.Actions;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSM.Tests
{
    public static class InProcWebServer
    {
        public static async Task JsonResultAsync(string uri, object result)
        {
            var completion = new TaskCompletionSource<bool>();

            RequestHandlerCallback callback = async ctx =>
            {
                var json = JsonConvert.SerializeObject(result);

                await ctx.SendStringAsync(json, "application/json", Encoding.UTF8);

                completion.SetResult(true);
            };

            var server = new WebServer(options => options.WithUrlPrefix(uri)
                                                         .WithMode(HttpListenerMode.EmbedIO))
                                                         .WithModule(new ActionModule("/", HttpVerbs.Any, callback));

            using (server)
            {
                server.RunAsync();

                await completion.Task;
            }
        }

        public static Task<string> EchoAsync(string uri)
        {
            return DelayEchoAsync(uri, TimeSpan.Zero);
        }

        public static async Task<string> DelayEchoAsync(string uri, TimeSpan delay)
        {
            var completion = new TaskCompletionSource<string>();

            RequestHandlerCallback callback = async ctx =>
            {
                await Task.Delay(delay);
                completion.SetResult(await ctx.GetRequestBodyAsStringAsync());
            };

            var server = new WebServer(options => options.WithUrlPrefix(uri)
                                                         .WithMode(HttpListenerMode.EmbedIO))
                                                         .WithModule(new ActionModule("/", HttpVerbs.Any, callback));

            using (server)
            {
                server.RunAsync();

                var result = await completion.Task;

                try
                {
                    return JsonConvert.DeserializeObject<string>(result);
                }
                catch
                {
                    return result;
                }
            }
        }
    }
}
