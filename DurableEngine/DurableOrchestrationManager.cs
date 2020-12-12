using DurableTask.Core;
using DurableTask.Core.Serializing;
using ImpromptuInterface;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Services;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal interface IStateChartOrchestrationManager
    {
        Task StartOrchestrationAsync(string instanceId, IRootStateMetadata metadata, Dictionary<string, object> data);
        Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId, TimeSpan timeout, CancellationToken token);
        Task SendMessageAsync(string instanceId, ExternalMessage message);
    }

    internal class DurableOrchestrationManager : IStateChartOrchestrationManager
    {
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly IOrchestrationService _orchestrationService;
        private readonly ILogger _logger;

        private ExecutionContext _context;
        private DurableStateChartService _service;

        public DurableOrchestrationManager(IOrchestrationService orchestrationService, ILogger logger = null)
        {
            orchestrationService.CheckArgNull(nameof(orchestrationService));

            _orchestrationService = orchestrationService;
            _logger = logger;

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", HttpService.PostAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", HttpService.GetAsync);
        }

        public Task StartAsync(ExecutionContext context, TimeSpan timeout, CancellationToken token)
        {
            context.CheckArgNull(nameof(context));

            _context = context;

            Func<string, ExternalServiceDelegate> getService = name =>
            {
                _externalServices.TryGetValue(name, out ExternalServiceDelegate func);
                return func;
            };

            Func<string, ExternalQueryDelegate> getQuery = name =>
            {
                _externalQueries.TryGetValue(name, out ExternalQueryDelegate func);
                return func;
            };

            _service = new DurableStateChartService(_orchestrationService, this, getService, getQuery, timeout, _logger);

            return _service.StartAsync(context.Metadata, token);
        }

        public async Task StopAsync()
        {
            if (_service != null)
            {
                await _service.StopAsync();
            }

            _service = null;
            _context = null;
        }

        public Task StartOrchestrationAsync()
        {
            if (_service == null)
            {
                throw new InvalidOperationException("Service is not running.");
            }

            return StartOrchestrationAsync(_context.Metadata.UniqueId, _context.Metadata, _context.Data);
        }

        public Task StartOrchestrationAsync(string instanceId, IRootStateMetadata metadata, Dictionary<string, object> data)
        {
            instanceId.CheckArgNull(nameof(instanceId));
            metadata.CheckArgNull(nameof(metadata));
            data.CheckArgNull(nameof(data));

            if (_service == null)
            {
                throw new InvalidOperationException("Orchestration service is not started.");
            }

            var client = new TaskHubClient((IOrchestrationServiceClient)_orchestrationService);

            return client.CreateOrchestrationInstanceAsync("statechart", instanceId, instanceId, data);
        }

        public async Task<IReadOnlyDictionary<string, object>> WaitForCompletionAsync(string instanceId, TimeSpan timeout, CancellationToken token)
        {
            if (_service == null)
            {
                throw new InvalidOperationException("Orchestration service is not started.");
            }

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            var result = await client.WaitForOrchestrationAsync(instance, timeout, token);

            var dataconverter = new JsonDataConverter(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var tuple = dataconverter.Deserialize<(Dictionary<string, object>, Exception)>(result.Output);

            if (tuple.Item2 != null)
            {
                ExceptionDispatchInfo.Capture(tuple.Item2).Throw();
                return null;
            }
            else
            {
                return tuple.Item1;
            }
        }

        public Task SendMessageAsync(string instanceId, ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            var instance = new OrchestrationInstance
            {
                InstanceId = instanceId
            };

            var client = new TaskHubClient((IOrchestrationServiceClient) _orchestrationService);

            return client.RaiseEventAsync(instance, message.Name, message);
        }
    }
}
