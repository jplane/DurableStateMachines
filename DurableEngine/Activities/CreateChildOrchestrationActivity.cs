using DurableTask.Core;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable.Activities
{
    internal class CreateChildOrchestrationActivity : AsyncTaskActivity<(string, Dictionary<string, object>), string>
    {
        private readonly IRootStateMetadata _metadata;
        private readonly IStateChartOrchestrationManager _orchestrationManager;

        public CreateChildOrchestrationActivity(IRootStateMetadata metadata,
                                                IStateChartOrchestrationManager orchestrationManager)
        {
            metadata.CheckArgNull(nameof(metadata));
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _metadata = metadata;
            _orchestrationManager = orchestrationManager;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, (string, Dictionary<string, object>) input)
        {
            var instanceId = input.Item1;
            var data = input.Item2;

            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));
            Debug.Assert(data != null);

            await _orchestrationManager.StartOrchestrationAsync(instanceId, _metadata, data);

            return string.Empty;
        }
    }
}
