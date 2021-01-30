using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using Nito.AsyncEx;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Exceptions;
using System.Threading;
using StateChartsDotNet.Services;
using StateChartsDotNet.Common.Model.Execution;
using Newtonsoft.Json.Linq;

namespace StateChartsDotNet
{
    public class ExecutionContext : ExecutionContextBase, IExecutionContext
    {
        private readonly AsyncProducerConsumerQueue<ExternalMessage> _externalMessages;
        private readonly AsyncLock _lock;
        private readonly Interpreter _interpreter;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly HttpService _http;

        private Task _executeTask;

        public ExecutionContext(IStateChartMetadata metadata,
                                object data,
                                CancellationToken cancelToken,
                                Func<string, IStateChartMetadata> lookupChild = null,
                                bool isChild = false,
                                ILogger logger = null)
            : base(metadata, data, cancelToken, lookupChild, isChild, logger)
        {
            _lock = new AsyncLock();
            _interpreter = new Interpreter();
            _externalMessages = new AsyncProducerConsumerQueue<ExternalMessage>();
            _http = new HttpService(this.ExecutionData, cancelToken);

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", _http.PostAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", _http.GetAsync);

            SetDataValue("_instanceId", this.GenerateGuid().ToString("N"));
        }

        public async Task StartAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_executeTask != null && !_executeTask.IsCompleted)
                {
                    throw new InvalidOperationException("StateChart instance is already running.");
                }

                _executeTask = _interpreter.RunAsync(this);
            }
        }

        public async Task WaitForCompletionAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_executeTask == null)
                {
                    throw new InvalidOperationException("StateChart instance is already running.");
                }

                await _executeTask;    // task is already bounded by token passed in StartAsync()
            }
        }

        public async Task StartAndWaitForCompletionAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_executeTask != null && !_executeTask.IsCompleted)
                {
                    throw new InvalidOperationException("StateChart instance is already running.");
                }

                _executeTask = _interpreter.RunAsync(this);

                await _executeTask;
            }
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

        internal override Task<string> QueryAsync(string activityType, IQueryConfiguration config)
        {
            activityType.CheckArgNull(nameof(activityType));

            if (_externalQueries.TryGetValue(activityType, out ExternalQueryDelegate query))
            {
                return query(config);
            }

            throw new InvalidOperationException("Unable to resolve external query type: " + activityType);
        }

        internal override Task SendMessageAsync(string activityType, string correlationId, ISendMessageConfiguration config)
        {
            activityType.CheckArgNull(nameof(activityType));
            config.CheckArgNull(nameof(config));

            if (_externalServices.TryGetValue(activityType, out ExternalServiceDelegate service))
            {
                return service(correlationId, config);
            }

            throw new InvalidOperationException("Unable to resolve external service type: " + activityType);
        }

        internal override async Task<object> InvokeChildStateChart(IInvokeStateChartMetadata metadata, string _)
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Remote)
            {
                throw new NotSupportedException("Remote child statechart execution not supported for in-memory engine.");
            }

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            var data = metadata.GetData(this.ExecutionData);

            var context = new ExecutionContext(childMachine, data, this.CancelToken, _lookupChild, true, _logger);

            await context.StartAndWaitForCompletionAsync();

            return data;
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
