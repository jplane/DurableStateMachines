using System;
using System.Collections.Generic;
using StateChartsDotNet.Model.States;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using Nito.AsyncEx;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Services;
using StateChartsDotNet.Common.Messages;

namespace StateChartsDotNet
{
    public class ExecutionContext
    {
        protected readonly Dictionary<string, object> _data;
        protected readonly ILogger _logger;
        protected readonly Dictionary<string, IRootStateMetadata> _childStatechartMetadata;

        private readonly Dictionary<string, (Task, ExecutionContext)> _childStatechartInstances;
        private readonly Dictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly Dictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly Dictionary<string, IEnumerable<State>> _historyValues;
        private readonly Queue<InternalMessage> _internalMessages;
        private readonly AsyncProducerConsumerQueue<ExternalMessage> _externalMessages;
        private readonly Set<State> _configuration;
        private readonly Set<State> _statesToInvoke;
        private readonly RootState _root;

        public ExecutionContext(IRootStateMetadata metadata, ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _root = new RootState(metadata);
            _logger = logger;

            _data = new Dictionary<string, object>();
            _historyValues = new Dictionary<string, IEnumerable<State>>();
            _internalMessages = new Queue<InternalMessage>();
            _externalMessages = new AsyncProducerConsumerQueue<ExternalMessage>();
            _configuration = new Set<State>();
            _statesToInvoke = new Set<State>();
            _childStatechartMetadata = new Dictionary<string, IRootStateMetadata>();
            _childStatechartInstances = new Dictionary<string, (Task, ExecutionContext)>();

            _externalServices = new Dictionary<string, ExternalServiceDelegate>();
            _externalServices.Add("http-post", HttpService.PostAsync);
            _externalServices.Add("send-parent", SendMessageToParentStateChart);
            _externalServices.Add("send-child", SendMessageToChildStateChart);

            _externalQueries = new Dictionary<string, ExternalQueryDelegate>();
            _externalQueries.Add("http-get", HttpService.GetAsync);
        }

        public bool IsRunning { get; internal set; }

        public object this[string key]
        {
            get { return _data[key]; }

            set
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException("Cannot set execution state while the state machine is running.");
                }

                _data[key] = value;
            }
        }

        public void ConfigureChildStateChart(IRootStateMetadata statechart)
        {
            statechart.CheckArgNull(nameof(statechart));

            _childStatechartMetadata[statechart.Id] = statechart;
        }

        public void ConfigureExternalQuery(string id, ExternalQueryDelegate handler)
        {
            id.CheckArgNull(nameof(id));
            handler.CheckArgNull(nameof(handler));

            _externalQueries[id] = handler;
        }

        public void ConfigureExternalService(string id, ExternalServiceDelegate handler)
        {
            id.CheckArgNull(nameof(id));
            handler.CheckArgNull(nameof(handler));

            _externalServices[id] = handler;
        }

        public void Stop()
        {
            Send("cancel");
        }

        public void Send(string message, object data = null)
        {
            message.CheckArgNull(nameof(message));

            var msg = new ExternalMessage(message)
            {
                Content = data
            };

            Send(msg);
        }

        public virtual void Send(ExternalMessage message)
        {
            _externalMessages.Enqueue(message);
        }

        private Task SendMessageToChildStateChart(string childId,
                                                  string messageName,
                                                  object content,
                                                  string _,
                                                  IReadOnlyDictionary<string, object> parameters)
        {
            var msg = new ExternalMessage(messageName)
            {
                Content = content,
                Parameters = parameters
            };

            SendToChildStateChart(childId, msg);

            return Task.CompletedTask;
        }

        private Task SendMessageToParentStateChart(string _,
                                                   string messageName,
                                                   object content,
                                                   string __,
                                                   IReadOnlyDictionary<string, object> parameters)
        {
            messageName.CheckArgNull(nameof(messageName));

            if (this.ParentContext == null)
            {
                throw new InvalidOperationException("Current statechart has no parent.");
            }

            var msg = new ChildStateChartResponseMessage(messageName)
            {
                CorrelationId = (string) _data["_invokeId"],
                Content = content,
                Parameters = parameters
            };

            this.ParentContext.Send(msg);

            return Task.CompletedTask;
        }

        internal void SendDoneMessageToParent(object content,
                                              IReadOnlyDictionary<string, object> parameters)
        {
            if (this.ParentContext != null)
            {
                var invokeId = (string) this["_invokeId"];

                Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));

                var msg = new ChildStateChartResponseMessage($"done.invoke.{invokeId}")
                {
                    CorrelationId = invokeId,
                    Content = content,
                    Parameters = parameters
                };

                this.ParentContext.Send(msg);
            }
        }

        internal ExecutionContext ParentContext { get; set; }

        internal ExternalQueryDelegate GetExternalQuery(string id)
        {
            id.CheckArgNull(nameof(id));

            if (_externalQueries.TryGetValue(id, out ExternalQueryDelegate query))
            {
                return query;
            }

            return null;
        }

        internal ExternalServiceDelegate GetExternalService(string id)
        {
            id.CheckArgNull(nameof(id));

            if (_externalServices.TryGetValue(id, out ExternalServiceDelegate service))
            {
                return service;
            }

            return null;
        }

        internal virtual Task ExecuteContentAsync(string uniqueId, Func<ExecutionContext, Task> func)
        {
            func.CheckArgNull(nameof(func));

            return func(this);
        }

        internal virtual void InvokeChildStateChart(IInvokeStateChartMetadata metadata, string invokeId)
        {
            metadata.CheckArgNull(nameof(metadata));
            invokeId.CheckArgNull(nameof(invokeId));

            Debug.Assert(! _childStatechartInstances.ContainsKey(invokeId));

            IRootStateMetadata childMachine;

            var rootId = metadata.GetRootId(this.ScriptData);

            if (!string.IsNullOrWhiteSpace(rootId))
            {
                if (!_childStatechartMetadata.TryGetValue(rootId, out childMachine))
                {
                    throw new InvalidOperationException($"Child statechart {rootId} not found.");
                }
            }
            else
            {
                childMachine = metadata.GetRoot(this.ScriptData);
            }

            if (childMachine == null)
            {
                throw new InvalidOperationException("Unable to resolve metadata for child statechart.");
            }

            var context = new ExecutionContext(childMachine);

            context.ParentContext = this;

            context["_invokeId"] = invokeId;

            foreach (var param in metadata.GetParams(this.ScriptData))
            {
                context[param.Key] = param.Value;
            }

            var interpreter = new Interpreter();

            var task = Task.Run(() => interpreter.RunAsync(context));

            _childStatechartInstances.Add(invokeId, (task, context));
        }

        internal async Task CancelInvokeAsync(string parentId, InvokeStateChart invoke)
        {
            parentId.CheckArgNull(nameof(parentId));
            invoke.CheckArgNull(nameof(invoke));

            foreach (var pair in _childStatechartInstances.Where(p => p.Key.StartsWith($"{parentId}.")).ToArray())
            {
                var invokeId = pair.Key;
                var task = pair.Value.Item1;
                var context = pair.Value.Item2;

                Debug.Assert(context != null);

                context.Stop();

                Debug.Assert(task != null);

                await task;

                _childStatechartInstances.Remove(invokeId);
            }
        }

        internal async Task ProcessExternalMessageAsync(string parentId, InvokeStateChart invoke, ExternalMessage message)
        {
            parentId.CheckArgNull(nameof(parentId));
            invoke.CheckArgNull(nameof(invoke));

            foreach (var pair in _childStatechartInstances.Where(p => p.Key.StartsWith($"{parentId}.")).ToArray())
            {
                var invokeId = pair.Key;

                await invoke.ProcessExternalMessageAsync(invokeId, this, message);
            }
        }

        internal virtual void ProcessChildStateChartDone(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                Debug.Assert(_childStatechartInstances.ContainsKey(message.CorrelationId));

                _childStatechartInstances.Remove(message.CorrelationId);
            }
        }

        internal virtual void SendToChildStateChart(string id, ExternalMessage message)
        {
            id.CheckArgNull(nameof(id));
            message.CheckArgNull(nameof(message));

            if (_childStatechartInstances.TryGetValue(id, out (Task, ExecutionContext) tuple))
            {
                var context = tuple.Item2;

                Debug.Assert(context != null);

                context.Send(message);
            }
            else
            {
                Debug.Fail($"Unable to find child statechart {id}.");
            }
        }

        internal virtual Task InitAsync()
        {
            this["_sessionid"] = Guid.NewGuid().ToString("D");

            this["_name"] = this.Root.Name;

            return Task.CompletedTask;
        }

        internal RootState Root => _root;

        internal async Task<ExternalMessage> DequeueExternalAsync()
        {
            var msg = await _externalMessages.DequeueAsync();

            _data["_event"] = msg;

            return msg;
        }

        internal DynamicDictionary ScriptData => new DynamicDictionary(_data);

        internal void SetDataValue(string key, object value)
        {
            _data[key] = value;
        }

        internal bool TryGet(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
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

            var evt = new InternalMessage("error.communication")
            {
                Data = ex
            };

            _internalMessages.Enqueue(evt);

            _logger?.LogError("Communication error", ex);
        }

        internal void EnqueueExecutionError(Exception ex)
        {
            Debug.Assert(ex != null);

            var evt = new InternalMessage("error.execution")
            {
                Data = ex
            };

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

        internal virtual Task LogDebugAsync(string message)
        {
            _logger?.LogDebug(message);

            return Task.CompletedTask;
        }

        internal virtual Task LogInformationAsync(string message)
        {
            _logger?.LogInformation(message);

            return Task.CompletedTask;
        }
    }
}
