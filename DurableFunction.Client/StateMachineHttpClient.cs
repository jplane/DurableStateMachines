using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Nito.AsyncEx;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableFunction.Client
{
    public class StateMachineHttpClient<TData> : HttpClient
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private StateMachineResponsePayload _response;

        public StateMachineHttpClient()
        {
        }

        public Task StartNewAsync(string stateMachineId, DebuggerInfo debugInfo = null)
        {
            return StartNewAsync(stateMachineId, default, debugInfo);
        }

        public async Task StartNewAsync(string stateMachineId,
                                        TData data,
                                        DebuggerInfo debugInfo = null)
        {
            stateMachineId.CheckArgNull(nameof(stateMachineId));

            if (this.BaseAddress == null)
            {
                throw new InvalidOperationException("HttpClient base address is not valid.");
            }

            using var _ = await _lock.LockAsync();

            var payload = new StateMachineRequestPayload<TData>
            {
                StateMachineIdentifier = stateMachineId,
                Arguments = data,
                DebugInfo = debugInfo
            };

            var content = new StringContent(payload.ToJson(), Encoding.UTF8, "application/json");

            var response = await this.PostAsync("/runtime/webhooks/durabletask/orchestrators/statemachine-orchestration", content);

            Debug.Assert(response != null);

            response.EnsureSuccessStatusCode();

            _response = StateMachineResponsePayload.FromJson(await response.Content.ReadAsStringAsync());
        }

        public async Task<DurableOrchestrationStatus> GetStatusAsync()
        {
            if (this.BaseAddress == null)
            {
                throw new InvalidOperationException("Base address URI is not valid.");
            }

            using var _ = await _lock.LockAsync();

            if (_response == null)
            {
                throw new InvalidOperationException("Start a new state machine instance before sending events.");
            }

            var uri = new Uri(_response.StatusQueryGetUri);

            var response = await this.GetAsync(uri);

            Debug.Assert(response != null);

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<DurableOrchestrationStatus>(await response.Content.ReadAsStringAsync());
        }

        public async Task SendEventAsync(string eventName, object content)
        {
            eventName.CheckArgNull(nameof(eventName));
            content.CheckArgNull(nameof(content));

            if (this.BaseAddress == null)
            {
                throw new InvalidOperationException("Base address URI is not valid.");
            }

            using var _ = await _lock.LockAsync();

            if (_response == null)
            {
                throw new InvalidOperationException("Start a new state machine instance before sending events.");
            }

            var eventContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

            var uri = new Uri(_response.SendEventPostUri);

            var response = await this.PostAsync(new Uri(uri, eventName), eventContent);

            Debug.Assert(response != null);

            response.EnsureSuccessStatusCode();
        }

        public async Task TerminateAsync()
        {
            if (this.BaseAddress == null)
            {
                throw new InvalidOperationException("Base address URI is not valid.");
            }

            using var _ = await _lock.LockAsync();

            if (_response == null)
            {
                throw new InvalidOperationException("Start a new state machine instance before sending events.");
            }

            var uri = new Uri(_response.TerminatePostUri);

            var response = await this.PostAsync(uri, null);

            Debug.Assert(response != null);

            response.EnsureSuccessStatusCode();
        }
    }
}
