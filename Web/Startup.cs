using DurableTask.AzureStorage;
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Web
{
    public class Startup
    {
        private IOrchestrationManager _manager;
        private CancellationToken _cancelToken;

        public Startup()
        {
            _cancelToken = CancellationToken.None;
        }

        public void Configure(IApplicationBuilder app, IConfiguration config, IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStarted.Register(OnAppStarted);
            lifetime.ApplicationStopped.Register(OnAppStopped);

            _cancelToken = lifetime.ApplicationStopping;

            ConfigureManager(config);

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

            app.MapWhen(context => IsJsonPost(context, "/api/register"), ab => ab.Run(ctxt => RegisterInstanceAsync(ctxt)));
            app.MapWhen(context => IsJsonPost(context, "/api/registerandstart"), ab => ab.Run(RegisterAndStartInstanceAsync));
            app.MapWhen(context => IsJsonPost(context, "/api/start"), ab => ab.Run(ctxt => StartInstanceAsync(ctxt)));
            app.MapWhen(context => IsJsonPut(context, "/api/stop"), ab => ab.Run(StopInstanceAsync));
            app.MapWhen(context => IsJsonPut(context, "/api/sendmessage"), ab => ab.Run(SendMessageToInstanceAsync));
            app.MapWhen(context => IsGet(context, "/api/status"), ab => ab.Run(GetInstanceStatusAsync));
        }

        private void ConfigureManager(IConfiguration config)
        {
            Debug.Assert(config != null);

            var timeout = TimeSpan.Parse(config["timeout"] ?? "00:01:00");

            var connectionString = config["storageConnectionString"];

            var settings = new AzureStorageOrchestrationServiceSettings
            {
                AppName = "StateChartsDotNet",
                TaskHubName = config["hubName"] ?? "default",
                StorageConnectionString = connectionString
            };

            var callbackUri = config["callbackUri"];

            var service = new AzureStorageOrchestrationService(settings);

            var storage = new DurableOrchestrationStorage(connectionString, _cancelToken);

            _manager = new DurableOrchestrationManager(service, storage, timeout, _cancelToken, callbackUri);
        }

        private void OnAppStopped()
        {
            Debug.Assert(_manager != null);

            _manager.StopAsync(false).Wait();
        }

        private void OnAppStarted()
        {
            Debug.Assert(_manager != null);

            _manager.StartAsync().Wait();
        }

        private async Task RegisterInstanceAsync(HttpContext context)
        {
            Debug.Assert(_manager != null);

            var json = await ReadJsonAsync(context.Request);

            Debug.Assert(json != null);

            var statechart = json["statechart"]?.Value<JObject>();

            if (statechart == null)
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart definition.");
            }

            var metadata = new StateChart(statechart);

            await _manager.RegisterAsync(metadata.MetadataId, metadata);

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = 201;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { metadataId = metadata.MetadataId }), _cancelToken);
        }

        private async Task StartInstanceAsync(HttpContext context)
        {
            Debug.Assert(_manager != null);

            var metadataId = context.Request.Query["metadataId"].ToString();

            if (string.IsNullOrWhiteSpace(metadataId))
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart metadataId.");
            }

            var json = await ReadJsonAsync(context.Request);

            Debug.Assert(json != null);

            var input = json["inputs"]?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();

            var instanceId = $"{metadataId}.{Guid.NewGuid():N}";

            await _manager.StartInstanceAsync(metadataId, instanceId, input);

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = 201;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { instanceId }), _cancelToken);
        }

        private async Task RegisterAndStartInstanceAsync(HttpContext context)
        {
            Debug.Assert(_manager != null);

            var json = await ReadJsonAsync(context.Request);

            Debug.Assert(json != null);

            var statechart = json["statechart"]?.Value<JObject>();

            if (statechart == null)
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart definition.");
            }

            var metadata = new StateChart(statechart);

            var metadataId = json["metadataId"]?.Value<string>() ?? metadata.MetadataId;

            await _manager.RegisterAsync(metadataId, metadata);

            var input = json["inputs"]?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();

            var instanceId = json["instanceId"]?.Value<string>() ?? $"{metadataId}.{Guid.NewGuid():N}";

            await _manager.StartInstanceAsync(metadataId, instanceId, input);

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = 201;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { instanceId }), _cancelToken);
        }

        private async Task StopInstanceAsync(HttpContext context)
        {
            Debug.Assert(_manager != null);

            var instanceId = context.Request.Query["instanceId"].ToString();

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart instanceId.");
            }
            else
            {
                await _manager.SendMessageAsync(instanceId, new ExternalMessage("cancel"));

                await _manager.WaitForInstanceAsync(instanceId);

                context.Response.StatusCode = 204;
            }
        }

        private async Task SendMessageToInstanceAsync(HttpContext context)
        {
            Debug.Assert(_manager != null);

            var json = await ReadJsonAsync(context.Request);

            Debug.Assert(json != null);

            var instanceId = context.Request.Query["instanceId"].ToString();

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new InvalidOperationException("HTTP payload does not contain statechart instanceId.");
            }
            else
            {
                var message = json.ToObject<ExternalMessage>();

                Debug.Assert(message != null);

                await _manager.SendMessageAsync(instanceId, message);

                context.Response.StatusCode = 204;
            }
        }

        private async Task GetInstanceStatusAsync(HttpContext context)
        {
            Debug.Assert(_manager != null);

            var instanceId = context.Request.Query["instanceId"].ToString();

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                throw new InvalidOperationException("HTTP query string does not contain statechart instanceId.");
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

                var state = await _manager.GetInstanceAsync(instanceId);

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
