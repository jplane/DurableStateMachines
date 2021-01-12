using DurableTask.AzureStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using StateChartsDotNet.Durable;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.WebHost
{
    public interface IOrchestrationManagerHostedService
    {
        IOrchestrationManager Manager { get; }
    }

    public class OrchestrationManagerHostedService : IHostedService, IOrchestrationManagerHostedService
    {
        private readonly IConfiguration _config;
        private readonly IHostApplicationLifetime _lifetime;

        private IOrchestrationManager _manager;
        private IOrchestrationStorage _storage;

        public OrchestrationManagerHostedService(IConfiguration config,
                                                 IHostApplicationLifetime lifetime)
        {
            _config = config;
            _lifetime = lifetime;
        }

        public IOrchestrationManager Manager => _manager;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var timeout = TimeSpan.Parse(_config["timeout"] ?? "00:01:00");

            var connectionString = _config["SCDN_STORAGE_CONNECTION_STRING"];

            var hubName = _config["SCDN_HUB_NAME"] ?? "default";

            var settings = new AzureStorageOrchestrationServiceSettings
            {
                AppName = "StateChartsDotNet",
                TaskHubName = hubName,
                StorageConnectionString = connectionString
            };

            var callbackUri = _config["SCDN_CALLBACK_URI"];

            var service = new AzureStorageOrchestrationService(settings);

            _storage = new DurableOrchestrationStorage(connectionString, hubName, _lifetime.ApplicationStopping);

            _manager = new DurableOrchestrationManager(service,
                                                       _storage,
                                                       timeout,
                                                       _lifetime.ApplicationStopping,
                                                       callbackUri);

            return _manager.StartAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.Assert(_manager != null);

            return _manager.StopAsync(false);
        }
    }
}
