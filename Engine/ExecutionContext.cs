﻿using System;
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
        private readonly AsyncLock _lock;
        private readonly Interpreter _interpreter;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly Dictionary<string, ExecutionContext> _childInstances;

        private Task _executeTask;
        private ExecutionContext _parentContext;

        public ExecutionContext(IRootStateMetadata metadata, CancellationToken cancelToken, ILogger logger = null)
            : base(metadata, cancelToken, logger)
        {
            _lock = new AsyncLock();
            _interpreter = new Interpreter();
            _childInstances = new Dictionary<string, ExecutionContext>();

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", HttpService.PostAsync);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", HttpService.GetAsync);

            _data["_invokeId"] = $"{metadata.UniqueId}.{Guid.NewGuid():N}";
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

        protected override Task SendMessageToParentStateChart(string _,
                                                              string messageName,
                                                              object content,
                                                              string __,
                                                              IReadOnlyDictionary<string, object> parameters,
                                                              CancellationToken ___)
        {
            messageName.CheckArgNull(nameof(messageName));

            if (_parentContext == null)
            {
                throw new ExecutionException("Statechart has no parent.");
            }

            var msg = new ChildStateChartResponseMessage(messageName)
            {
                CorrelationId = (string) _data["_invokeId"],
                Content = content,
                Parameters = parameters
            };

            return _parentContext.SendAsync(msg);
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
                return query(target, parameters, this.CancelToken);
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

            switch (type)
            {
                case "send-parent":
                    messageName.CheckArgNull(nameof(messageName));
                    return SendMessageToParentStateChart(null, messageName, content, null, parameters, this.CancelToken);

                case "send-child":
                    messageName.CheckArgNull(nameof(messageName));
                    return SendMessageToChildStateChart(target, messageName, content, null, parameters, this.CancelToken);
            }

            if (_externalServices.TryGetValue(type, out ExternalServiceDelegate service))
            {
                return service(target, messageName, content, correlationId, parameters, this.CancelToken);
            }

            throw new InvalidOperationException("Unable to resolve external service type: " + type);
        }

        internal override Task ExecuteScriptAsync(IScriptMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Execute(this.ScriptData);

            return Task.CompletedTask;
        }

        protected override bool IsChildStateChart => _parentContext != null;

        internal override async Task InvokeChildStateChart(IInvokeStateChartMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var childMachine = ResolveChildStateChart(metadata);

            Debug.Assert(childMachine != null);

            var context = new ExecutionContext(childMachine, this.CancelToken, _logger);

            context._parentContext = this;

            var invokeId = $"{metadata.UniqueId}.{await GenerateGuid():N}";

            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));

            context._data["_invokeId"] = invokeId;

            foreach (var param in metadata.GetParams(this.ScriptData))
            {
                context._data[param.Key] = param.Value;
            }

            await context.StartAsync();

            _childInstances.Add(invokeId, context);
        }

        internal override async Task CancelInvokesAsync(string parentUniqueId)
        {
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));

            foreach (var pair in _childInstances.Where(p => p.Key.StartsWith($"{parentUniqueId}.")).ToArray())
            {
                var context = pair.Value;

                Debug.Assert(context != null);

                await context.SendStopMessageAsync();
            }
        }

        internal override IEnumerable<string> GetInvokeIdsForParent(string parentUniqueId)
        {
            return _childInstances.Where(p => p.Key.StartsWith($"{parentUniqueId}."))
                                  .Select(p => p.Key)
                                  .ToArray();
        }

        internal override async Task ProcessChildStateChartDoneAsync(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                if (_childInstances.Remove(message.CorrelationId, out ExecutionContext context))
                {
                    await context.WaitForCompletionAsync();
                }
                else
                {
                    Debug.Fail("Expected to find child state machine instance: " + message.CorrelationId);
                }
            }
        }

        internal override async Task SendToChildStateChart(string id, ExternalMessage message)
        {
            id.CheckArgNull(nameof(id));
            message.CheckArgNull(nameof(message));

            if (_childInstances.TryGetValue(id, out ExecutionContext context))
            {
                await context.SendAsync(message);
            }
            else
            {
                Debug.Fail("Expected to find child state machine instance: " + id);
            }
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
