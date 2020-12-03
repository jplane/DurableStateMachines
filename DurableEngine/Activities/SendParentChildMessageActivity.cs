using DurableTask.Core;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable.Activities
{
    internal class SendParentChildMessageActivity : AsyncTaskActivity<(string, ExternalMessage), string>
    {
        private readonly IStateChartOrchestrationManager _orchestrationManager;

        public SendParentChildMessageActivity(IStateChartOrchestrationManager orchestrationManager)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, (string, ExternalMessage) input)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(input.Item1));
            Debug.Assert(input.Item2 != null);

            await _orchestrationManager.SendMessageAsync(input.Item1, input.Item2);

            return string.Empty;
        }
    }
}
