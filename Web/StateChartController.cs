using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Durable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.Web
{
    [ApiController]
    [Route("api")]
    public class StateChartController : ControllerBase
    {
        private readonly IOrchestrationManager _manager;

        public StateChartController(IOrchestrationManagerHostedService service)
        {
            _manager = service.Manager;
        }

        [HttpGet]
        [Route("metadata/{metadataId}")]
        [ActionName("GetMetadataAsync")]
        public Task<ActionResult> GetMetadataAsync(string metadataId)
        {
            return Task.FromResult((ActionResult)new OkResult());
        }

        [HttpPost]
        [Route("register")]
        [ActionName("RegisterAsync")]
        public async Task<ActionResult> RegisterAsync(IStateChartMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            Debug.Assert(_manager != null);

            await _manager.RegisterAsync(metadata.MetadataId, metadata);

            var result = new { metadataId = metadata.MetadataId };

            return CreatedAtAction(nameof(GetMetadataAsync), result, result);
        }

        [HttpPost]
        [Route("start/{metadataId}")]
        [ActionName("StartInstanceAsync")]
        public async Task<IActionResult> StartInstanceAsync(string metadataId,
                                                            Dictionary<string, object> parameters)
        {
            metadataId.CheckArgNull(nameof(metadataId));

            var instanceId = await StartAsync(metadataId, null, parameters);

            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            var result = new { instanceId };

            return CreatedAtAction(nameof(GetInstanceAsync), result, result);
        }

        [HttpPost]
        [Route("registerandstart")]
        [ActionName("RegisterAndStartInstanceAsync")]
        public async Task<IActionResult> RegisterAndStartInstanceAsync(RegisterAndStartPayload payload)
        {
            payload.CheckArgNull(nameof(payload));

            if (string.IsNullOrWhiteSpace(payload.MetadataId))
            {
                payload.MetadataId = payload.Metadata.MetadataId;
            }

            await _manager.RegisterAsync(payload.MetadataId, payload.Metadata);

            var instanceId = await StartAsync(payload.MetadataId, payload.InstanceId, payload.Parameters);

            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            return CreatedAtAction(nameof(GetInstanceAsync), new { instanceId }, new { metadataId = payload.MetadataId, instanceId });
        }

        [HttpPut]
        [Route("stop/{instanceId}")]
        [ActionName("StopInstanceAsync")]
        public async Task<ActionResult> StopInstanceAsync(string instanceId)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            Debug.Assert(_manager != null);

            await _manager.SendMessageAsync(instanceId, new ExternalMessage { Name = "cancel" });

            await _manager.WaitForInstanceAsync(instanceId);

            return Ok();
        }

        [HttpPut]
        [Route("sendmessage/{instanceId}")]
        [ActionName("SendMessageToInstanceAsync")]
        public async Task<ActionResult> SendMessageToInstanceAsync(string instanceId, ExternalMessage message)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            message.CheckArgNull(nameof(message));

            Debug.Assert(_manager != null);

            await _manager.SendMessageAsync(instanceId, message);

            return Ok();
        }

        [HttpGet("status/{instanceId}")]
        [ActionName("GetInstanceAsync")]
        public async Task<ActionResult> GetInstanceAsync(string instanceId)
        {
            instanceId.CheckArgNull(nameof(instanceId));

            Debug.Assert(_manager != null);

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

            return Ok(output);
        }

        private async Task<string> StartAsync(string metadataId,
                                              string instanceId = null,
                                              IDictionary<string, object> parameters = null)
        {
            metadataId.CheckArgNull(nameof(metadataId));

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                instanceId = $"{metadataId}.{Guid.NewGuid():N}";
            }

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            await _manager.StartInstanceAsync(metadataId, instanceId, parameters);

            return instanceId;
        }
    }
}
