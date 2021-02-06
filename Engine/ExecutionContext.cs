using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using DSM.Common;
using Nito.AsyncEx;
using DSM.Common.Model.States;
using DSM.Common.Messages;
using System.Threading;
using DSM.Engine.Services;
using DSM.Common.Model.Actions;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DSM.Engine
{
    public sealed class ExecutionContext : ExecutionContextBase
    {
        private readonly AsyncProducerConsumerQueue<ExternalMessage> _externalMessages;
        private readonly Interpreter _interpreter;
        private readonly Lazy<Dictionary<string, ExternalServiceDelegate>> _externalServices;
        private readonly Lazy<Dictionary<string, ExternalQueryDelegate>> _externalQueries;
        private readonly Lazy<HttpService> _http;

        public ExecutionContext(IStateMachineMetadata metadata,
                                CancellationToken cancelToken,
                                Func<string, IStateMachineMetadata> lookupChild = null,
                                string[] instanceIds = null,
                                ILogger logger = null)
            : base(metadata, cancelToken, lookupChild, instanceIds, logger)
        {
            _interpreter = new Interpreter();
            _externalMessages = new AsyncProducerConsumerQueue<ExternalMessage>();

            _http = new Lazy<HttpService>(() => new HttpService(this.ExecutionData, cancelToken));

            _externalServices = new Lazy<Dictionary<string, ExternalServiceDelegate>>(() =>
            {
                var services = new Dictionary<string, ExternalServiceDelegate>();
                services.Add("http-post", _http.Value.PostAsync);
                return services;
            });

            _externalQueries = new Lazy<Dictionary<string, ExternalQueryDelegate>>(() =>
            {
                var queries = new Dictionary<string, ExternalQueryDelegate>();
                queries.Add("http-get", _http.Value.GetAsync);
                return queries;
            });
        }

        public async Task<TData> RunAsync<TData>(TData data)
        {
            data.CheckArgNull(nameof(data));

            _data = data;

            this.InstanceId = this.GenerateGuid().ToString("N");

            await _interpreter.RunAsync(this);

            return (TData) _data;
        }

        protected override Task SendAsync(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            _externalMessages.Enqueue(message);

            return Task.CompletedTask;
        }

        internal override Task DelayAsync(TimeSpan timespan)
        {
            Debug.Assert(timespan > TimeSpan.Zero);

            return Task.Delay(timespan, this.CancelToken);
        }

        internal override Task<string> QueryAsync(string activityType, (object, JObject) config)
        {
            activityType.CheckArgNull(nameof(activityType));

            Debug.Assert(config.Item1 != null);
            Debug.Assert(config.Item2 == null);

            if (_externalQueries.Value.TryGetValue(activityType, out ExternalQueryDelegate query))
            {
                return query(config.Item1);
            }

            throw new InvalidOperationException("Unable to resolve external query type: " + activityType);
        }

        internal override Task SendMessageAsync(string activityType, string correlationId, (object, JObject) config)
        {
            activityType.CheckArgNull(nameof(activityType));
            config.CheckArgNull(nameof(config));

            Debug.Assert(config.Item1 != null);
            Debug.Assert(config.Item2 == null);

            if (_externalServices.Value.TryGetValue(activityType, out ExternalServiceDelegate service))
            {
                return service(correlationId, config.Item1);
            }

            throw new InvalidOperationException("Unable to resolve external service type: " + activityType);
        }

        internal override Task<object> InvokeChildStateMachine(IInvokeStateMachineMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.ExecutionMode == ChildStateMachineExecutionMode.Remote)
            {
                throw new NotSupportedException("Remote child statemachine execution not supported for in-memory engine.");
            }

            var childMachine = ResolveChildStateMachine(metadata);

            Debug.Assert(childMachine != null);

            var data = metadata.GetData(this.ExecutionData);

            var context = new ExecutionContext(childMachine, this.CancelToken, _lookupChild, this.InstanceIdStack, _logger);

            return context.RunAsync(data);
        }

        protected override Task<ExternalMessage> GetNextExternalMessageAsync()
        {
            return _externalMessages.DequeueAsync(this.CancelToken);
        }

        protected override Guid GenerateGuid()
        {
            return Guid.NewGuid();
        }

        internal override Task LogDebugAsync(string message)
        {
            _logger?.LogDebug(message);

            return Task.CompletedTask;
        }

        internal override Task LogInformationAsync(string message)
        {
            _logger?.LogInformation(message);

            return Task.CompletedTask;
        }
    }
}
