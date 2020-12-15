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
        private readonly CancellationToken _cancelToken;

        public WaitForCompletionActivity(IOrchestrationManager orchestrationManager, CancellationToken cancelToken)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
            _cancelToken = cancelToken;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string instanceId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));

            await _orchestrationManager.WaitForCompletionAsync(instanceId, _cancelToken);

            return string.Empty;
        }
    }
}
