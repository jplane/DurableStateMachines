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

namespace StateChartsDotNet
{
    public abstract class ExecutionContextBase<TData>
    {
        protected readonly ILogger _logger;

        private readonly Dictionary<string, IEnumerable<State<TData>>> _historyValues;
        private readonly Queue<InternalMessage> _internalMessages;
        private readonly Set<State<TData>> _configuration;
        private readonly Set<State<TData>> _statesToInvoke;
        private readonly StateChart<TData> _root;
        private readonly IStateChartMetadata _metadata;

        private TData _data;
        private IDictionary<string, object> _internalData;
        private CancellationToken _cancelToken;
        private Exception _error;
        private bool _isRunning = false;

        internal ExecutionContextBase(IStateChartMetadata metadata,
                                      TData data,
                                      CancellationToken cancelToken,
                                      ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Validate();

            _metadata = metadata;
            _data = data;
            _root = new StateChart<TData>(metadata);
            _cancelToken = cancelToken;
            _logger = logger;

            _internalData = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State<TData>>>();
            _internalMessages = new Queue<InternalMessage>();
            _configuration = new Set<State<TData>>();
            _statesToInvoke = new Set<State<TData>>();
        }

        internal abstract Task DelayAsync(TimeSpan timespan);

        internal abstract Task<string> QueryAsync(string activityType, IQueryConfiguration config);

        internal abstract Task SendMessageAsync(string activityType, string correlationId, ISendMessageConfiguration config);

        internal abstract Task<TData> InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentStateMetadataId);

        internal abstract Task LogDebugAsync(string message);

        internal abstract Task LogInformationAsync(string message);

        protected abstract Guid GenerateGuid();

        protected abstract Task<ExternalMessage> GetNextExternalMessageAsync();

        protected bool IsChildStateChart => _internalData.ContainsKey("_ischild");

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

            var childMachine = metadata.GetRoot();

            if (childMachine == null)
            {
                throw new ExecutionException("Unable to resolve metadata for child statechart.");
            }

            return childMachine;
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

        internal StateChart<TData> Root => _root;

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

        internal DynamicDictionary<TData> ExecutionData => new DynamicDictionary<TData>(_internalData, _data);

        internal void SetDataValue(string key, object value)
        {
            ((dynamic) this.ExecutionData)[key] = value;
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

        internal Set<State<TData>> Configuration => _configuration;

        internal Set<State<TData>> StatesToInvoke => _statesToInvoke;

        internal bool TryGetHistoryValue(string key, out IEnumerable<State<TData>> value)
        {
            return _historyValues.TryGetValue(key, out value);
        }

        internal void StoreHistoryValue(string key, Func<State<TData>, bool> predicate)
        {
            predicate.CheckArgNull(nameof(predicate));

            _historyValues[key] = this.Configuration.Where(predicate).ToArray();
        }
    }
}
