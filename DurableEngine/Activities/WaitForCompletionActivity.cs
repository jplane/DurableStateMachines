using DurableTask.Core;
using StateChartsDotNet.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable.Activities
{
    internal class WaitForCompletionActivity : AsyncTaskActivity<string, string>
    {
        private readonly IStateChartOrchestrationManager _orchestrationManager;
        private readonly TimeSpan _timeout;
        private readonly CancellationToken _token;

        public WaitForCompletionActivity(IStateChartOrchestrationManager orchestrationManager,
                                         TimeSpan timeout,
                                         CancellationToken token)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
            _timeout = timeout;
            _token = token;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string instanceId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            await _orchestrationManager.WaitForCompletionAsync(instanceId, _timeout, _token);

            return string.Empty;
        }
    }
}
