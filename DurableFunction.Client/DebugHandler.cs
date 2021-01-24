using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using StateChartsDotNet.Common.Debugger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunction.Client
{
    public abstract class DebugHandler
    {
        private readonly AsyncLock _lock = new AsyncLock();
        
        private TaskCompletionSource<bool> _tcs;
        private HubConnection _conn;

        protected DebugHandler()
        {
        }

        public DebuggerInfo GetDebuggerInfo()
        {
            return new DebuggerInfo
            {
                DebugInstructions = this.SupportedInstructions.ToArray(),
                DebugUri = this.DebuggerUri
            };
        }

        public string DebuggerUri { get; set; }

        public async Task StartAsync()
        {
            using (await _lock.LockAsync())
            {
                if (_conn != null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(this.DebuggerUri))
                {
                    throw new InvalidOperationException("DebuggerUri property is invalid.");
                }

                Debug.Assert(_tcs == null);

                _tcs = new TaskCompletionSource<bool>();

                _conn = new HubConnectionBuilder()
                                    .WithUrl(this.DebuggerUri)
                                    .AddNewtonsoftJsonProtocol()
                                    .Build();
            }

            Func<IDictionary<string, object>, Task> handler = data =>
            {
                var action = (DebuggerAction) Enum.Parse(typeof(DebuggerAction), (string) data["_debuggeraction"]);

                Task task = null;

                switch (action)
                {
                    case DebuggerAction.EnterStateMachine:
                        task = OnEnterStateMachine(data);
                        break;

                    case DebuggerAction.ExitStateMachine:
                        task = OnExitStateMachine(data);
                        break;

                    case DebuggerAction.EnterState:
                        task = OnEnterState(data);
                        break;

                    case DebuggerAction.ExitState:
                        task = OnExitState(data);
                        break;

                    case DebuggerAction.MakeTransition:
                        task = OnMakeTransition(data);
                        break;

                    case DebuggerAction.BeforeAction:
                        task = OnBeforeAction(data);
                        break;

                    case DebuggerAction.AfterAction:
                        task = OnAfterAction(data);
                        break;

                    case DebuggerAction.BeforeInvokeChildStateMachine:
                        task = OnBeforeInvokeChildStateMachine(data);
                        break;

                    case DebuggerAction.AfterInvokeChildStateMachine:
                        task = OnAfterInvokeChildStateMachine(data);
                        break;
                }

                return task;
            };

            using (_conn.On("break", handler))
            {
                await _conn.StartAsync();
                await _tcs.Task;
            }

            await _conn.StopAsync();
        }

        public Task StopAsync()
        {
            _tcs?.SetResult(true);
            return Task.CompletedTask;
        }

        protected virtual IEnumerable<DebuggerInstruction> SupportedInstructions
        {
            get
            {
                var actions = (IEnumerable<DebuggerAction>) Enum.GetValues(typeof(DebuggerAction));

                Debug.Assert(actions != null);

                return actions.Select(a => new DebuggerInstruction { Action = a, Element = "*" });
            }
        }

        protected virtual Task OnEnterStateMachine(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnExitStateMachine(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnEnterState(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnExitState(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnMakeTransition(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnBeforeAction(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnAfterAction(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnBeforeInvokeChildStateMachine(IDictionary<string, object> data)
        {
            return Resume();
        }

        protected virtual Task OnAfterInvokeChildStateMachine(IDictionary<string, object> data)
        {
            return Resume();
        }

        private Task Resume()
        {
            Debug.Assert(_conn != null);
            return _conn.SendAsync("resume");
        }
    }
}
