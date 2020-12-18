using DurableTask.Core;
using DurableTask.Emulator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Durable;
using StateChartsDotNet.Metadata.Json.States;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Web
{
    public class Startup
    {
        private readonly ConcurrentDictionary<string, IOrchestrationManager> _instances;
        private readonly IOrchestrationService _orchestrationService;

        private CancellationToken _cancelToken;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _cancelToken = CancellationToken.None;
            _orchestrationService = new LocalOrchestrationService();
            _instances = new ConcurrentDictionary<string, IOrchestrationManager>();
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStarted.Register(OnAppStarted);
            lifetime.ApplicationStopped.Register(OnAppStopped);

            _cancelToken = lifetime.ApplicationStopping;

            bool IsJsonPost(HttpContext context, string path)
            {
                return context.Request.Method.ToLowerInvariant() == "post" &&
                       context.Request.ContentType.ToLowerInvariant() == "application/json" &&
                       context.Request.Path.StartsWithSegments(path);
            }

            bool IsJsonPut(HttpContext context, string path)
            {
                return context.Request.Method.ToLowerInvariant() == "put" &&
                       context.Request.ContentType.ToLowerInvariant() == "application/json" &&
                       context.Request.Path.StartsWithSegments(path);
            }

            bool IsGet(HttpContext context, string path)
            {
                return context.Request.Method.ToLowerInvariant() == "get" &&
                       context.Request.Path.StartsWithSegments(path);
            }

            app.MapWhen(context => IsJsonPost(context, "/api/start"), ab => ab.Run(StartInstanceAsync));
            app.MapWhen(context => IsJsonPut(context, "/api/stop"), ab => ab.Run(StopInstanceAsync));
            app.MapWhen(context => IsJsonPost(context, "/api/sendmessage"), ab => ab.Run(SendMessageToInstanceAsync));
            app.MapWhen(context => IsGet(context, "/api/status"), ab => ab.Run(GetInstanceStatusAsync));
        }

        private void OnAppStopped()
        {
            foreach (var instance in _instances.Values)
            {
                instance.StopAsync().Wait();
            }

            _instances.Clear();

            _orchestrationService.StopAsync(false).Wait();
        }

        private void OnAppStarted()
        {
            _orchestrationService.StartAsync().Wait();

            // restart any not-yet-complete instances
        }

        private async Task StartInstanceAsync(HttpContext context)
        {
            var json = await ReadJsonAsync(context.Request);

            Debug.Assert(json != null);

            var statechart = json["statechart"]?.Value<JObject>();

            if (statechart == null)
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart definition.");
            }

            var metadata = new StateChart(statechart);

            var input = json["inputs"]?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();

            var timeout = json["timeout"]?.ToObject<TimeSpan>() ?? TimeSpan.FromMinutes(5);

            var manager = new DurableOrchestrationManager(metadata, _orchestrationService, timeout, _cancelToken);

            var instanceId = Guid.NewGuid().ToString("N");

            _instances.TryAdd(instanceId, manager);

            await manager.StartAsync();

            await manager.StartOrchestrationAsync(metadata.UniqueId, instanceId, input);

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = 201;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { instanceId }), _cancelToken);
        }

        private async Task StopInstanceAsync(HttpContext context)
        {
            var instanceId = context.Request.Query["instanceId"].ToString();

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart instanceId.");
            }
            else if (! _instances.TryGetValue(instanceId, out IOrchestrationManager manager))
            {
                throw new InvalidOperationException("Unable to find statechart instance for instanceId: " + instanceId);
            }
            else
            {
                await manager.SendMessageAsync(instanceId, new ExternalMessage("cancel"));

                await manager.WaitForCompletionAsync(instanceId);

                context.Response.StatusCode = 204;
            }
        }

        private async Task SendMessageToInstanceAsync(HttpContext context)
        {
            var json = await ReadJsonAsync(context.Request);

            Debug.Assert(json != null);

            var instanceId = context.Request.Query["instanceId"].ToString();

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart instanceId.");
            }
            else if (!_instances.TryGetValue(instanceId, out IOrchestrationManager manager))
            {
                throw new InvalidOperationException("Unable to find statechart instance for instanceId: " + instanceId);
            }
            else
            {
                var message = json.ToObject<ExternalMessage>();

                Debug.Assert(message != null);

                await manager.SendMessageAsync(instanceId, message);

                context.Response.StatusCode = 204;
            }
        }

        private async Task GetInstanceStatusAsync(HttpContext context)
        {
            var instanceId = context.Request.Query["instanceId"].ToString();

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new InvalidOperationException("HTTP query string does not contain statechart instanceId.");
            }
            else if (!_instances.TryGetValue(instanceId, out IOrchestrationManager manager))
            {
                throw new InvalidOperationException("Unable to find statechart instance for instanceId: " + instanceId);
            }
            else
            {
                Dictionary<string, object> DeserializeInput(string fragment)
                {
                    if (string.IsNullOrWhiteSpace(fragment))
                    {
                        return new Dictionary<string, object>();
                    }

                    var settings = new JsonSerializerSettings 
                    {
                        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii 
                    };

                    return JsonConvert.DeserializeObject<Dictionary<string, object>>(fragment, settings);
                }

                (Dictionary<string, object>, Exception) DeserializeOutput(string fragment)
                {
                    if (string.IsNullOrWhiteSpace(fragment))
                    {
                        return (new Dictionary<string, object>(), null);
                    }

                    var settings = new JsonSerializerSettings
                    {
                        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                    };

                    return JsonConvert.DeserializeObject<(Dictionary<string, object>, Exception)>(fragment, settings);
                }

                var state = await manager.GetStateAsync(instanceId);

                var result = DeserializeOutput(state.Output);

                var output = new
                {
                    startTime = state.CreatedTime,
                    endTime = state.CompletedTime,
                    lastUpdateTime = state.LastUpdatedTime,
                    status = state.OrchestrationStatus.ToString(),
                    instanceId = instanceId,
                    input = DeserializeInput(state.Input),
                    output = result.Item1,
                    error = result.Item2
                };

                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonConvert.SerializeObject(output), _cancelToken);
            }
        }

        private async Task<JObject> ReadJsonAsync(HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);

            var json = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Unexpected empty HTTP request payload.");
            }

            return JObject.Parse(json);
        }
    }
}
