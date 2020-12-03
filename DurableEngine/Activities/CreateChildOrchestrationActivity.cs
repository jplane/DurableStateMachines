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
            Debug.Assert(!string.IsNullOrWhiteSpace(input.Item1));
            Debug.Assert(input.Item2 != null);

            await _orchestrationManager.StartOrchestrationAsync(input.Item1, _metadata, input.Item2);

            return string.Empty;
        }
    }
}
