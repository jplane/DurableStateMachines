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

namespace StateChartsDotNet
{
    public abstract class ExecutionContextBase : IExecutionContext
    {
        protected readonly Dictionary<string, object> _data;
        protected readonly ILogger _logger;
        protected readonly AsyncProducerConsumerQueue<ExternalMessage> _externalMessages;

        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<InternalMessage> _internalMessages;
        private readonly Set<State> _configuration;
        private readonly Set<State> _statesToInvoke;
        private readonly RootState _root;

        private CancellationToken _cancelToken;
        private Exception _error;
        private bool _isRunning = false;

        internal ExecutionContextBase(IRootStateMetadata metadata,
                                      ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _root = new RootState(metadata);
            _logger = logger;
            _externalMessages = new AsyncProducerConsumerQueue<ExternalMessage>();

            _data = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalMessages = new Queue<InternalMessage>();
            _configuration = new Set<State>();
            _statesToInvoke = new Set<State>();

        }

        internal abstract Task CancelInvokesAsync(string parentUniqueId);

        internal abstract IEnumerable<string> GetInvokeIdsForParent(string parentUniqueId);

        internal abstract Task ProcessChildStateChartDoneAsync(ChildStateChartResponseMessage message);

        internal abstract Task SendToChildStateChart(string id, ExternalMessage message);

        internal abstract void InternalCancel();

        internal abstract Task DelayAsync(TimeSpan timespan);

        internal abstract Task<string> QueryAsync(string type, string target, IReadOnlyDictionary<string, object> parameters);

        internal abstract Task SendMessageAsync(string type,
                                                string target,
                                                string messageName,
                                                object content,
                                                string correlationId,
                                                IReadOnlyDictionary<string, object> parameters);

        internal abstract Task ExecuteScriptAsync(IScriptMetadata metadata);

        internal abstract Task InvokeChildStateChart(IInvokeStateChartMetadata metadata);

        internal abstract Task LogDebugAsync(string message);

        internal abstract Task LogInformationAsync(string message);

        protected abstract Task<Guid> GenerateGuid();

        protected abstract Task SendMessageToParentStateChart(string _,
                                                              string messageName,
                                                              object content,
                                                              string __,
                                                              IReadOnlyDictionary<string, object> parameters,
                                                              CancellationToken ___);

        protected abstract bool IsChildStateChart { get; }

        public Task StopAsync()
        {
            return SendAsync("cancel");
        }

        public Task SendAsync(string message,
                              object content = null,
                              IReadOnlyDictionary<string, object> parameters = null)
        {
            message.CheckArgNull(nameof(message));

            var msg = new ExternalMessage(message)
            {
                Content = content,
                Parameters = parameters
            };

            return SendAsync(msg);
        }

        public Task SendAsync(ExternalMessage message)
        {
            message.CheckArgNull(nameof(message));

            _externalMessages.Enqueue(message);

            return Task.CompletedTask;
        }

        public bool IsRunning
        {
            get => _isRunning && (!this.FailFast || _error == null);
        }

        public object this[string key]
        {
            get
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot read execution state while the state machine is running.");
                }

                if (key.StartsWith("_"))
                {
                    throw new KeyNotFoundException($"Value for key '{key}' not found.");
                }

                return _data[key];
            }

            set
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot write execution state while the state machine is running.");
                }

                _data[key] = value;
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

        protected Task SendMessageToChildStateChart(string childId,
                                                    string messageName,
                                                    object content,
                                                    string _,
                                                    IReadOnlyDictionary<string, object> parameters,
                                                    CancellationToken __)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(childId));
            Debug.Assert(!string.IsNullOrWhiteSpace(messageName));

            var msg = new ExternalMessage(messageName)
            {
                Content = content,
                Parameters = parameters
            };

            return SendToChildStateChart(childId, msg);
        }

        internal void EnterFinalRootState()
        {
            _isRunning = false;
        }

        internal bool FailFast => _root.FailFast;

        internal Task SendDoneMessageToParentAsync(object content,
                                                   IReadOnlyDictionary<string, object> parameters)
        {
            if (IsChildStateChart)
            {
                var invokeId = _data["_invokeId"];

                if (_error != null)
                {
                    return SendMessageToParentStateChart(null,
                                                         $"done.invoke.error.{invokeId}",
                                                         _error,
                                                         null,
                                                         null,
                                                         _cancelToken);
                }
                else
                {
                    return SendMessageToParentStateChart(null,
                                                         $"done.invoke.{invokeId}",
                                                         content,
                                                         null,
                                                         parameters,
                                                         _cancelToken);
                }
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public async Task<string> ResolveSendMessageId(ISendMessageMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            var id = metadata.Id;

            if (string.IsNullOrWhiteSpace(id))
            {
                id = (await this.GenerateGuid()).ToString("N");

                await this.LogDebugAsync($"Synthentic Id = {id}");

                if (!string.IsNullOrWhiteSpace(metadata.IdLocation))
                {
                    _data[metadata.IdLocation] = id;
                }
            }

            return id;
        }

        protected IRootStateMetadata ResolveChildStateChart(IInvokeStateChartMetadata metadata)
        {
            Debug.Assert(metadata != null);

            var childMachine = metadata.GetRoot();

            if (childMachine == null)
            {
                throw new ExecutionException("Unable to resolve metadata for child statechart.");
            }

            return childMachine;
        }

        internal async Task InitAsync(CancellationToken cancelToken)
        {
            _cancelToken = cancelToken;

            _data["_name"] = this.Root.Name;

            _isRunning = true;

            if (this.Root.Binding == Databinding.Early)
            {
                await this.Root.InitDatamodel(this, true);
            }

            await this.Root.ExecuteScript(this);
        }

        internal RootState Root => _root;

        internal CancellationToken CancelToken => _cancelToken;

        protected virtual Task<ExternalMessage> GetNextExternalMessageAsync()
        {
            return _externalMessages.DequeueAsync(_cancelToken);
        }

        internal async Task<ExternalMessage> DequeueExternalAsync()
        {
            ExternalMessage msg;

            try
            {
                msg = await GetNextExternalMessageAsync();
            }
            catch (TaskCanceledException)
            {
                msg = new ExternalMessage("cancel");
            }

            _data["_event"] = msg;

            if (msg.IsCancel)
            {
                _isRunning = false;
            }
            else if (msg is ChildStateChartResponseMessage rm && rm.IsInvokeError)
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

        internal void EnqueueInternal(string message)
        {
            message.CheckArgNull(nameof(message));

            var evt = new InternalMessage(message);

            _internalMessages.Enqueue(evt);
        }

        internal void EnqueueCommunicationError(Exception ex)
        {
            Debug.Assert(ex != null);

            if (! (ex is CommunicationException))
            {
                ex = new CommunicationException("A communication error occurred during statechart processing.", ex);
            }

            var evt = new InternalMessage("error.communication")
            {
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

            var evt = new InternalMessage("error.execution")
            {
                Content = ex
            };

            _error = ex;

            _internalMessages.Enqueue(evt);

            _logger?.LogError("Execution error", ex);
        }

        internal bool HasInternalMessages => _internalMessages.Count > 0;

        internal InternalMessage DequeueInternal()
        {
            if (_internalMessages.TryDequeue(out InternalMessage evt))
            {
                _data["_event"] = evt;
            }

            return evt;
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
