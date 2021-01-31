using System;
using System.Collections.Generic;
using StateChartsDotNet.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Exceptions;
using System.Runtime.ExceptionServices;
using System.Threading;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Debugger;
using System.Net.WebSockets;

namespace StateChartsDotNet
{
    public abstract class ExecutionContextBase
    {
        protected readonly ILogger _logger;
        protected readonly Func<string, IStateChartMetadata> _lookupChild;

        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<InternalMessage> _internalMessages;
        private readonly Set<State> _configuration;
        private readonly Set<State> _statesToInvoke;
        private readonly StateChart _root;
        private readonly IStateChartMetadata _metadata;
        private readonly bool _isChild;

        private object _data;
        private IDictionary<string, object> _internalData;
        private CancellationToken _cancelToken;
        private Exception _error;
        private bool _isRunning = false;

        internal ExecutionContextBase(IStateChartMetadata metadata,
                                      object data,
                                      CancellationToken cancelToken,
                                      Func<string, IStateChartMetadata> lookupChild = null,
                                      bool isChild = false,
                                      ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Validate();

            _metadata = metadata;
            _data = data;
            _root = new StateChart(metadata);
            _cancelToken = cancelToken;
            _logger = logger;
            _lookupChild = lookupChild;
            _isChild = isChild;

            _internalData = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalMessages = new Queue<InternalMessage>();
            _configuration = new Set<State>();
            _statesToInvoke = new Set<State>();
        }

        internal abstract Task DelayAsync(TimeSpan timespan);

        internal abstract Task<string> QueryAsync(string activityType, IQueryConfiguration config);

        internal abstract Task SendMessageAsync(string activityType, string correlationId, ISendMessageConfiguration config);

        internal abstract Task<object> InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentStateMetadataId);

        internal abstract Task LogDebugAsync(string message);

        internal abstract Task LogInformationAsync(string message);

        protected abstract Guid GenerateGuid();

        protected abstract Task<ExternalMessage> GetNextExternalMessageAsync();

        protected bool IsChildStateChart => _isChild;

        protected IReadOnlyDictionary<string, object> GetDebuggerValues()
        {
            var dict = new Dictionary<string, object>(_internalData);

            dict.Add("executionstate", _data);

            return dict;
        }

        internal virtual Task BreakOnDebugger(DebuggerAction action, IModelMetadata metadata)
        {
            return Task.CompletedTask;
        }

        internal void InternalCancel()
        {
            if (!IsChildStateChart)
            {
                _isRunning = false;
            }
        }

        public Task SendStopMessageAsync()
        {
            return SendMessageAsync("cancel");
        }

        public Task SendMessageAsync(string message,
                                     object content = null,
                                     IReadOnlyDictionary<string, object> parameters = null)
        {
            message.CheckArgNull(nameof(message));

            var msg = new ExternalMessage
            {
                Name = message,
                Content = content,
                Parameters = parameters
            };

            return SendAsync(msg);
        }

        protected virtual Task SendAsync(ExternalMessage message)
        {
            return Task.CompletedTask;
        }

        public bool IsRunning
        {
            get => _isRunning && (!this.FailFast || _error == null);
        }

        internal void CheckErrorPropagation()
        {
            if (this.FailFast && _error != null)
            {
                Debug.Assert(_error is StateChartException);

                ExceptionDispatchInfo.Capture(_error).Throw();
            }
        }

        internal void EnterFinalRootState()
        {
            _isRunning = false;
        }

        internal bool FailFast => _root.FailFast;

        public async Task<string> ResolveSendMessageId(ISendMessageMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var id = metadata.Id;

            if (string.IsNullOrWhiteSpace(id))
            {
                id = this.GenerateGuid().ToString("N");

                await this.LogDebugAsync($"Synthentic Id = {id}");

                if (!string.IsNullOrWhiteSpace(metadata.IdLocation))
                {
                    _internalData[metadata.IdLocation] = id;
                }
            }

            return id;
        }

        protected IStateChartMetadata ResolveChildStateChart(IInvokeStateChartMetadata metadata)
        {
            Debug.Assert(metadata != null);

            return metadata.GetRoot() ??
                   _lookupChild?.Invoke(metadata.GetRootIdentifier()) ??
                   throw new InvalidOperationException($"Unable to resolve child state machine: {metadata.MetadataId}");
        }

        internal async Task InitAsync()
        {
            _internalData["_name"] = this.Root.Name;

            await this.BreakOnDebugger(DebuggerAction.EnterStateMachine, _metadata);

            _isRunning = true;

            await this.Root.ExecuteScript(this);
        }

        internal Task ExitAsync()
        {
            return this.BreakOnDebugger(DebuggerAction.ExitStateMachine, _metadata);
        }

        internal StateChart Root => _root;

        internal CancellationToken CancelToken => _cancelToken;

        internal async Task<ExternalMessage> DequeueExternalAsync()
        {
            ExternalMessage msg;

            try
            {
                msg = await GetNextExternalMessageAsync();
            }
            catch (TaskCanceledException)
            {
                msg = new ExternalMessage { Name = "cancel" };
            }

            _internalData["_event"] = msg;

            if (msg.IsCancel)
            {
                _isRunning = false;
            }

            return msg;
        }

        internal DynamicDictionary ExecutionData => new DynamicDictionary(_internalData, _data);

        internal void SetInternalDataValue(string key, object value)
        {
            _internalData[key] = value;
        }

        internal void SetDataValue(string key, object value)
        {
            dynamic data = this.ExecutionData;
            data[key] = value;
        }

        internal void EnqueueInternal(string message)
        {
            message.CheckArgNull(nameof(message));

            var evt = new InternalMessage { Name = message };

            _internalMessages.Enqueue(evt);
        }

        internal void EnqueueCommunicationError(Exception ex)
        {
            Debug.Assert(ex != null);

            if (! (ex is CommunicationException))
            {
                ex = new CommunicationException("A communication error occurred during statechart processing.", ex);
            }

            var evt = new InternalMessage
            {
                Name = "error.communication",
                Content = ex
            };

            _error = ex;

            _internalMessages.Enqueue(evt);

            _logger?.LogError("Communication error", ex);
        }

        internal void EnqueueExecutionError(Exception ex)
        {
            Debug.Assert(ex != null);

            if (! (ex is ExecutionException))
            {
                ex = new ExecutionException("An error occurred during statechart processing.", ex);
            }

            var evt = new InternalMessage
            {
                Name = "error.execution",
                Content = ex
            };

            _error = ex;

            _internalMessages.Enqueue(evt);

            _logger?.LogError("Execution error", ex);
        }

        internal bool HasInternalMessages => _internalMessages.Count > 0;

        internal InternalMessage DequeueInternal()
        {
            InternalMessage msg = null;

            try
            {
                msg = _internalMessages.Dequeue();
            }
            catch (InvalidOperationException)
            {
                // if queue is empty, return null
            }

            _internalData["_event"] = msg;

            return msg;
        }

        internal Set<State> Configuration => _configuration;

        internal Set<State> StatesToInvoke => _statesToInvoke;

        internal bool TryGetHistoryValue(string key, out IEnumerable<State> value)
        {
            return _historyValues.TryGetValue(key, out value);
        }

        internal void StoreHistoryValue(string key, Func<State, bool> predicate)
        {
            predicate.CheckArgNull(nameof(predicate));

            _historyValues[key] = this.Configuration.Where(predicate).ToArray();
        }
    }
}
