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
        private readonly CancellationToken _token;

        public WaitForCompletionActivity(IStateChartOrchestrationManager orchestrationManager,
                                         CancellationToken token)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
            _token = token;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string input)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(input));

            await _orchestrationManager.WaitForCompletionAsync(input, TimeSpan.FromSeconds(30) /* TODO: configure */, _token);

            return string.Empty;
        }
    }
}
