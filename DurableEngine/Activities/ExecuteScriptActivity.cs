using DurableTask.Core;
using System;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace StateChartsDotNet.Durable.Activities
{
    internal class ExecuteScriptActivity : AsyncTaskActivity<Dictionary<string, object>, object>
    {
        private readonly IScriptMetadata _metadata;

        public ExecuteScriptActivity(IScriptMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        protected override Task<object> ExecuteAsync(TaskContext context, Dictionary<string, object> input)
        {
            Debug.Assert(input != null);

            var data = new DynamicDictionary(input);

            _metadata.Execute(data);

            return Task.FromResult((object) 0);
        }
    }
}
