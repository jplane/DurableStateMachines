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
        private readonly IOrchestrationManager _orchestrationManager;

        public WaitForCompletionActivity(IOrchestrationManager orchestrationManager)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string instanceId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            await _orchestrationManager.WaitForCompletionAsync(instanceId);

            return string.Empty;
        }
    }
}
