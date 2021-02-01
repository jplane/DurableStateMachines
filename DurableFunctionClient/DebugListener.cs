using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using StateChartsDotNet.Common.Debugger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunctionClient
{
    public abstract class DebugListener
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private TaskCompletionSource<bool> _tcs;

        protected DebugListener()
        {
        }

        public string DebuggerUri { get; set; }

        public virtual DebuggerInfo DebuggerInfo => new DebuggerInfo
        {
            Instructions = this.AllInstructions.ToArray()
        };

        public async Task StartAsync(string instanceId)
        {
            using (await _lock.LockAsync())
            {
                if (string.IsNullOrWhiteSpace(this.DebuggerUri))
                {
                    throw new InvalidOperationException("DebuggerUri property is invalid.");
                }

                Debug.Assert(_tcs == null);

                _tcs = new TaskCompletionSource<bool>();

                var conn = new HubConnectionBuilder()
                                    .WithUrl(this.DebuggerUri)
                                    .AddNewtonsoftJsonProtocol()
                                    .Build();

                Func<IDictionary<string, object>, Task> onBreak = async data =>
                {
                    await OnDebuggerBreak(data);
                    await conn.SendAsync("resume");
                };

                try
                {
                    using (conn.On("break", onBreak))
                    {
                        await conn.StartAsync();
                        await conn.SendAsync("register", instanceId);
                        await _tcs.Task;
                    }
                }
                finally
                {
                    await conn.SendAsync("unregister", instanceId);
                    await conn.StopAsync();
                }
            }
        }

        public void Stop()
        {
            _tcs?.SetResult(true);
        }

        private Task OnDebuggerBreak(IDictionary<string, object> data)
        {
            var action = (DebuggerAction)Enum.Parse(typeof(DebuggerAction), (string)data["_debuggeraction"]);

            Task task;

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

                default:
                    Debug.Fail("Unexpected debugger action: " + action);
                    task = Task.CompletedTask;
                    break;
            }

            return task;
        }

        private IEnumerable<DebuggerInstruction> AllInstructions
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
