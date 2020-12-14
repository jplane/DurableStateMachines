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

        public WaitForCompletionActivity(IStateChartOrchestrationManager orchestrationManager, TimeSpan timeout)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
            _timeout = timeout;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string instanceId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            await _orchestrationManager.WaitForCompletionAsync(instanceId, _timeout);

            return string.Empty;
        }
    }
}
