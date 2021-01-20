using System;
using System.Collections.Generic;
using StateChartsDotNet.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Services;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Exceptions;
using System.Runtime.ExceptionServices;
using System.Threading;
using StateChartsDotNet.Common.Model.Execution;
using Nito.AsyncEx;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Debugger;

namespace StateChartsDotNet
{
    public abstract class ExecutionContextBase
    {
        protected readonly ILogger _logger;

        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<InternalMessage> _internalMessages;
        private readonly Set<State> _configuration;
        private readonly Set<State> _statesToInvoke;
        private readonly StateChart _root;
        private readonly IStateChartMetadata _metadata;

        private IDictionary<string, object> _data;
        private CancellationToken _cancelToken;
        private Exception _error;
        private bool _isRunning = false;

        internal ExecutionContextBase(IStateChartMetadata metadata,
                                      CancellationToken cancelToken,
                                      ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _root = new StateChart(metadata);
            _cancelToken = cancelToken;
            _logger = logger;

            _data = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalMessages = new Queue<InternalMessage>();
            _configuration = new Set<State>();
            _statesToInvoke = new Set<State>();
        }

        internal abstract Task DelayAsync(TimeSpan timespan);

        internal abstract Task<string> QueryAsync(string activityType, IQueryConfiguration config);

        internal abstract Task SendMessageAsync(string activityType, string correlationId, ISendMessageConfiguration config);

        internal abstract Task InvokeChildStateChart(IInvokeStateChartMetadata metadata, string parentStateMetadataId);

        internal abstract Task LogDebugAsync(string message);

        internal abstract Task LogInformationAsync(string message);

        protected abstract Guid GenerateGuid();

        protected abstract bool IsChildStateChart { get; }

        protected abstract Task<ExternalMessage> GetNextExternalMessageAsync();

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

        public IDictionary<string, object> Data
        {
            get
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot read execution state while the state machine is running.");
                }

                return new ExternalDictionary(_data);
            }
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
                    _data[metadata.IdLocation] = id;
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
            _data["_name"] = this.Root.Name;

            _isRunning = true;

            if (this.Root.Binding == Databinding.Early)
            {
                await this.Root.InitDatamodel(this, true);
            }

            await this.Root.ExecuteScript(this);

            await this.BreakOnDebugger(DebuggerAction.EnterStateMachine, _metadata);
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

            _data["_event"] = msg;

            if (msg.IsCancel)
            {
                _isRunning = false;
            }
            else if (msg.IsChildStateChartResponse && msg.IsInvokeError)
            {
                Debug.Assert(msg.Content != null);
                Debug.Assert(msg.Content is Exception);

                _error = (Exception) msg.Content;
            }

            return msg;
        }

        internal DynamicDictionary ScriptData => new DynamicDictionary(_data);

        internal void SetDataValue(string key, object value)
        {
            _data[key] = value;
        }

        internal IEnumerable<KeyValuePair<string, object>> GetDataValues() => _data;

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

            _data["_event"] = msg;

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
