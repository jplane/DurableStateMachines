using DurableTask.Core;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable.Activities
{
    internal class CreateChildOrchestrationActivity : AsyncTaskActivity<(string, Dictionary<string, object>), string>
    {
        private readonly IStateChartMetadata _metadata;
        private readonly string _uniqueId;
        private readonly IOrchestrationManager _orchestrationManager;

        public CreateChildOrchestrationActivity(IStateChartMetadata metadata,
                                                string uniqueId,
                                                IOrchestrationManager orchestrationManager)
        {
            metadata.CheckArgNull(nameof(metadata));
            uniqueId.CheckArgNull(nameof(uniqueId));
            orchestrationManager.CheckArgNull(nameof(orchestrationManager));

            _metadata = metadata;
            _uniqueId = uniqueId;
            _orchestrationManager = orchestrationManager;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, (string, Dictionary<string, object>) input)
        {
            var instanceId = input.Item1;
            var data = input.Item2;

            Debug.Assert(!string.IsNullOrWhiteSpace(instanceId));
            Debug.Assert(data != null);

            await _orchestrationManager.StartOrchestrationAsync(_metadata, _uniqueId, instanceId, data);

            return string.Empty;
        }
    }
}
