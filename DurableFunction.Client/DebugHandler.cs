using Microsoft.AspNetCore.SignalR.Client;
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

                _conn = new HubConnectionBuilder().WithUrl(this.DebuggerUri).Build();
            }

            Func<IDictionary<string, object>, Task> handler = data =>
            {
                var action = (DebuggerAction) data["_debuggeraction"];

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

        private Task Resume()
        {
            Debug.Assert(_conn != null);
            return _conn.SendAsync("resume");
        }
    }
}
