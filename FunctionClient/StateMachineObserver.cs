using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSM.Common.Observability;

namespace DSM.FunctionClient
{
    public abstract class StateMachineObserver : IAsyncDisposable   // not IDisposable cuz need async to clean up HubConnection
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly string _instanceId;
        
        private HubConnection _conn;
        private IDisposable _handle;

        protected StateMachineObserver()
        {
            _instanceId = Guid.NewGuid().ToString("N");
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_handle is not null)
            {
                if (_handle is IAsyncDisposable disposable)
                {
                    await disposable.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    _handle.Dispose();
                }
            }

            _handle = null;

            if (_conn is not null)
            {
                await _conn.SendAsync("unregister", _instanceId);
                await _conn.StopAsync();
            }

            _conn = null;
        }

        public string EndpointUri { get; set; }

        internal string InstanceId => _instanceId;

        public virtual Instruction[] Instructions
        {
            get
            {
                var actions = (IEnumerable<ObservableAction>)Enum.GetValues(typeof(ObservableAction));

                Debug.Assert(actions != null);

                return actions.Select(a => new Instruction { Action = a, Element = "*" }).ToArray();
            }
        }

        internal async Task StartAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_conn != null)
                {
                    throw new InvalidOperationException("Observer is already listening for state machine events.");
                }

                if (string.IsNullOrWhiteSpace(this.EndpointUri))
                {
                    throw new InvalidOperationException("EndpointUri property is invalid.");
                }

                _conn = new HubConnectionBuilder()
                                    .WithUrl(this.EndpointUri)
                                    .AddNewtonsoftJsonProtocol()
                                    .Build();

                Func<IDictionary<string, object>, Task> onBreak = async data =>
                {
                    await OnBreak(data);
                    await _conn.SendAsync("resume", _instanceId);
                };

                _handle = _conn.On("break", onBreak);

                await _conn.StartAsync();

                await _conn.SendAsync("register", _instanceId);
            }
        }

        private Task OnBreak(IDictionary<string, object> data)
        {
            var action = (ObservableAction) Enum.Parse(typeof(ObservableAction), (string) data["_action"]);

            Task task;

            switch (action)
            {
                case ObservableAction.EnterStateMachine:
                    task = OnEnterStateMachine(data);
                    break;

                case ObservableAction.ExitStateMachine:
                    task = OnExitStateMachine(data);
                    break;

                case ObservableAction.EnterState:
                    task = OnEnterState(data);
                    break;

                case ObservableAction.ExitState:
                    task = OnExitState(data);
                    break;

                case ObservableAction.MakeTransition:
                    task = OnMakeTransition(data);
                    break;

                case ObservableAction.BeforeAction:
                    task = OnBeforeAction(data);
                    break;

                case ObservableAction.AfterAction:
                    task = OnAfterAction(data);
                    break;

                case ObservableAction.BeforeInvokeChildStateMachine:
                    task = OnBeforeInvokeChildStateMachine(data);
                    break;

                case ObservableAction.AfterInvokeChildStateMachine:
                    task = OnAfterInvokeChildStateMachine(data);
                    break;

                default:
                    Debug.Fail("Unexpected debugger action: " + action);
                    task = Task.CompletedTask;
                    break;
            }

            return task;
        }

        protected virtual Task OnEnterStateMachine(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnExitStateMachine(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnEnterState(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnExitState(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMakeTransition(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnBeforeAction(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnAfterAction(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnBeforeInvokeChildStateMachine(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnAfterInvokeChildStateMachine(IDictionary<string, object> data)
        {
            return Task.CompletedTask;
        }
    }
}
