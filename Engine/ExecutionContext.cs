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
        private ExecutionContext _parentContext;

        public ExecutionContext(IStateChartMetadata metadata, CancellationToken cancelToken, ILogger logger = null)
            : base(metadata, cancelToken, logger)
        {
            metadata.Validate();

            _lock = new AsyncLock();
            _interpreter = new Interpreter();
            _externalMessages = new AsyncProducerConsumerQueue<ExternalMessage>();
            _http = new HttpService(cancelToken);

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", _http.PostAsync);
            _externalServices.Add("http-put", _http.PutAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", _http.GetAsync);

            SetDataValue("_instanceId", $"{metadata.MetadataId}.{Guid.NewGuid():N}");
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

        internal override Task<string> QueryAsync(string type, string target, IReadOnlyDictionary<string, object> parameters)
        {
            type.CheckArgNull(nameof(type));
            target.CheckArgNull(nameof(target));
            parameters.CheckArgNull(nameof(parameters));

            if (_externalQueries.TryGetValue(type, out ExternalQueryDelegate query))
            {
                return query(target, parameters);
            }

            throw new InvalidOperationException("Unable to resolve external query type: " + type);
        }

        internal override Task SendMessageAsync(string type,
                                                string target,
                                                string messageName,
                                                object content,
                                                string correlationId,
                                                IReadOnlyDictionary<string, object> parameters)
        {
            type.CheckArgNull(nameof(type));
            target.CheckArgNull(nameof(target));
            parameters.CheckArgNull(nameof(parameters));

            if (_externalServices.TryGetValue(type, out ExternalServiceDelegate service))
            {
                return service(target, messageName, content, correlationId, parameters);
            }

            throw new InvalidOperationException("Unable to resolve external service type: " + type);
        }

        protected override bool IsChildStateChart => _parentContext != null;

        internal override async Task InvokeChildStateChart(IInvokeStateChartMetadata metadata, string _)
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.ExecutionMode == ChildStateChartExecutionMode.Remote)
            {
                throw new NotSupportedException("Remote child statechart execution not supported for in-memory engine.");
            }

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            var context = new ExecutionContext(childMachine, this.CancelToken, _logger);

            context._parentContext = this;

            var instanceId = $"{metadata.MetadataId}.{await GenerateGuid():N}";

            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            context.SetDataValue("_instanceId", instanceId);

            foreach (var param in metadata.GetParams(this.ScriptData))
            {
                context.SetDataValue(param.Key, param.Value);
            }

            await context.StartAndWaitForCompletionAsync();

            if (!string.IsNullOrWhiteSpace(metadata.ResultLocation))
            {
                SetDataValue(metadata.ResultLocation, (IReadOnlyDictionary<string, object>) context.Data);
            }
        }

        protected override Task<ExternalMessage> GetNextExternalMessageAsync()
        {
            return _externalMessages.DequeueAsync(this.CancelToken);
        }

        protected override Task<Guid> GenerateGuid()
        {
            return Task.FromResult(Guid.NewGuid());
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
