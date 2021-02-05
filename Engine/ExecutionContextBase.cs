using System;
using System.Collections.Generic;
using DSM.Engine.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using DSM.Common;
using DSM.Common.Model.States;
using DSM.Common.Messages;
using DSM.Common.Model;
using DSM.Common.Exceptions;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSM.Common.Observability;

namespace DSM.Engine
{
    public abstract class ExecutionContextBase
    {
        protected readonly ILogger _logger;
        protected readonly Func<string, IStateMachineMetadata> _lookupChild;

        protected object _data;

        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<InternalMessage> _internalMessages;
        private readonly Set<State> _configuration;
        private readonly Set<State> _statesToInvoke;
        private readonly StateMachine _root;
        private readonly IStateMachineMetadata _metadata;
        private readonly string[] _parentInstanceIds;

        private IDictionary<string, object> _internalData;
        private CancellationToken _cancelToken;
        private Exception _error;
        private bool _isRunning = false;

        internal ExecutionContextBase(IStateMachineMetadata metadata,
                                      CancellationToken cancelToken,
                                      Func<string, IStateMachineMetadata> lookupChild = null,
                                      string[] parentInstanceIds = null,
                                      ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Validate();

            _metadata = metadata;
            _root = new StateMachine(metadata);
            _cancelToken = cancelToken;
            _logger = logger;
            _lookupChild = lookupChild;
            _parentInstanceIds = parentInstanceIds;

            _internalData = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalMessages = new Queue<InternalMessage>();
            _configuration = new Set<State>();
            _statesToInvoke = new Set<State>();
        }

        internal abstract Task DelayAsync(TimeSpan timespan);

        internal abstract Task<string> QueryAsync(string activityType, (object, JObject) config);

        internal abstract Task SendMessageAsync(string activityType, string correlationId, (object, JObject) config);

        internal abstract Task<object> InvokeChildStateMachine(IInvokeStateMachineMetadata metadata);

        internal abstract Task LogDebugAsync(string message);

        internal abstract Task LogInformationAsync(string message);

        protected abstract Guid GenerateGuid();

        protected abstract Task<ExternalMessage> GetNextExternalMessageAsync();

        protected bool IsChildStateMachine => _parentInstanceIds?.Any() ?? false;

        protected IReadOnlyDictionary<string, object> GetDebuggerValues()
        {
            var dict = new Dictionary<string, object>(_internalData);

            dict.Add("executionstate", JsonConvert.SerializeObject(_data));

            return dict;
        }

        internal virtual Task OnAction(ObservableAction action, IModelMetadata metadata)
        {
            return Task.CompletedTask;
        }

        internal void InternalCancel()
        {
            if (!IsChildStateMachine)
            {
                _isRunning = false;
            }
        }

        public Task TerminateAsync()
        {
            return SendMessageAsync("cancel");
        }

        public Task SendMessageAsync(string message, object content = null)
        {
            message.CheckArgNull(nameof(message));

            var msg = new ExternalMessage
            {
                Name = message,
                Content = content
            };

            return SendAsync(msg);
        }

        protected virtual Task SendAsync(ExternalMessage message)
        {
            return Task.CompletedTask;
        }

        internal bool IsRunning
        {
            get => _isRunning && (!this.FailFast || _error == null);
        }

        internal void CheckErrorPropagation()
        {
            if (this.FailFast && _error != null)
            {
                Debug.Assert(_error is StateMachineException);

                ExceptionDispatchInfo.Capture(_error).Throw();
            }
        }

        internal void EnterFinalRootState()
        {
            _isRunning = false;
        }

        internal bool FailFast => _root.FailFast;

        protected IStateMachineMetadata ResolveChildStateMachine(IInvokeStateMachineMetadata metadata)
        {
            Debug.Assert(metadata != null);

            return metadata.GetRoot() ??
                   _lookupChild?.Invoke(metadata.GetRootIdentifier()) ??
                   throw new InvalidOperationException($"Unable to resolve child state machine: {metadata.MetadataId}");
        }

        internal async Task InitAsync()
        {
            _internalData["_name"] = this.Root.Name;

            await this.OnAction(ObservableAction.EnterStateMachine, _metadata);

            _isRunning = true;

            await this.Root.ExecuteScript(this);
        }

        internal Task ExitAsync()
        {
            return this.OnAction(ObservableAction.ExitStateMachine, _metadata);
        }

        internal StateMachine Root => _root;

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

        protected string[] InstanceIdStack =>
            new[] { this.InstanceId }.Concat(_parentInstanceIds ?? Enumerable.Empty<string>()).ToArray();

        protected string InstanceId
        {
            get => (string) _internalData["_instanceId"];
            set => _internalData["_instanceId"] = value;
        }

        internal void SetInternalDataValue(string key, object value)
        {
            _internalData[key] = value;
        }

        internal void SetDataValue((string name, MemberInfo member) key, object value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(key.name) || key.member != null);

            dynamic data = this.ExecutionData;

            if (!string.IsNullOrWhiteSpace(key.name))
            {
                data[key.name] = value;
            }
            else
            {
                data[key.member] = value;
            }
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
                ex = new CommunicationException("A communication error occurred during statemachine processing.", ex);
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
                ex = new ExecutionException("An error occurred during statemachine processing.", ex);
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
