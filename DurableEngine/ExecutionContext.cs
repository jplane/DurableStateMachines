using DurableTask.Core;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    public class ExecutionContext : IExecutionContext, IInstanceManager
    {
        private readonly AsyncLock _lock;
        private readonly IRootStateMetadata _metadata;
        private readonly IOrchestrationManager _orchestrationManager;

        private Dictionary<string, object> _data;

        public ExecutionContext(IRootStateMetadata metadata,
                                IOrchestrationService service,
                                TimeSpan timeout,
                                ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));
            service.CheckArgNull(nameof(service));

            _metadata = metadata;
            _orchestrationManager = new DurableOrchestrationManager(service, timeout, logger);

            _lock = new AsyncLock();
            _data = new Dictionary<string, object>();
        }

        public Task StartAsync()
        {
            return StartAsync(CancellationToken.None);
        }

        public async Task StartAsync(CancellationToken token)
        {
            Debug.Assert(_orchestrationManager != null);

            using (_lock.Lock())
            {
                if (_data.ContainsKey("_invokeId"))
                {
                    throw new InvalidOperationException("StateChart instance is already running.");
                }

                await _orchestrationManager.StartAsync();

                var instanceId = $"{_metadata.UniqueId}.{Guid.NewGuid():N}";

                _data["_invokeId"] = instanceId;

                await _orchestrationManager.StartOrchestrationAsync(_metadata, _metadata.UniqueId, instanceId, _data, token, true);
            } 
        }

        public Task WaitForCompletionAsync()
        {
            return WaitForCompletionAsync(CancellationToken.None);
        }

        public async Task WaitForCompletionAsync(CancellationToken token)
        {
            Debug.Assert(_orchestrationManager != null);

            using (await _lock.LockAsync())
            {
                try
                {
                    if (!_data.ContainsKey("_invokeId"))
                    {
                        throw new InvalidOperationException("StateChart instance is not running.");
                    }

                    var instanceId = (string) _data["_invokeId"];

                    var output = await _orchestrationManager.WaitForCompletionAsync(instanceId, token);

                    Debug.Assert(output != null);

                    _data = new Dictionary<string, object>(output);
                }
                catch (TimeoutException)
                {
                    // cancellation token fired, we want to eat this one here
                }
                finally
                {
                    _data.Remove("_invokeId");

                    await _orchestrationManager.StopAsync();
                }
            }
        }

        public IDictionary<string, object> Data => new ExternalDictionary(_data);

        public async Task SendMessageAsync(string message,
                                           object content = null,
                                           IReadOnlyDictionary<string, object> parameters = null)
        {
            message.CheckArgNull(nameof(message));

            Debug.Assert(_orchestrationManager != null);

            using (await _lock.LockAsync())
            {
                if (!_data.ContainsKey("_invokeId"))
                {
                    throw new InvalidOperationException("StateChart instance is not running.");
                }

                var instanceId = (string)_data["_invokeId"];

                Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

                var msg = new ExternalMessage(message)
                {
                    Content = content,
                    Parameters = parameters
                };

                await _orchestrationManager.SendMessageAsync(instanceId, msg);
            }
        }

        public Task SendStopMessageAsync()
        {
            return SendMessageAsync("cancel");
        }
    }
}
