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
        private readonly IOrchestrationManager _orchestrationManager;

        public SendParentChildMessageActivity(IOrchestrationManager orchestrationManager)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, (string, ExternalMessage) input)
        {
            var instanceId = input.Item1;
            var message = input.Item2;

            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));
            Debug.Assert(message != null);

            await _orchestrationManager.SendMessageAsync(instanceId, message);

            return string.Empty;
        }
    }
}
