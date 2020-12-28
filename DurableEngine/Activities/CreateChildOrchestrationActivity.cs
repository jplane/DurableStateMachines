using DurableTask.Core;
using StateChartsDotNet.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable.Activities
{
    internal class CreateChildOrchestrationActivity : AsyncTaskActivity<(string, string, Dictionary<string, object>), string>
    {
        private readonly IOrchestrationManager _orchestrationManager;

        public CreateChildOrchestrationActivity(IOrchestrationManager orchestrationManager)
        {
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _orchestrationManager = orchestrationManager;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, (string, string, Dictionary<string, object>) input)
        {
            var metadataId = input.Item1;
            var instanceId = input.Item2;
            var data = input.Item3;

            Debug.Assert(!string.IsNullOrWhiteSpace(metadataId));
            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));
            Debug.Assert(data != null);

            await _orchestrationManager.StartInstanceAsync(metadataId, instanceId, data);

            return string.Empty;
        }
    }
}
