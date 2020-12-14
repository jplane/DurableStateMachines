using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class InlineDurableExecutionContext : DurableExecutionContext
    {
        public InlineDurableExecutionContext(IRootStateMetadata metadata,
                                             OrchestrationContext orchestrationContext,
                                             IDictionary<string, object> data,
                                             ILogger logger = null)
            : base(metadata, orchestrationContext, data, logger)
        {
        }

        protected override Task StartChildOrchestrationAsync(string invokeId, Dictionary<string, object> data)
        {
            invokeId.CheckArgNull(nameof(invokeId));
            data.CheckArgNull(nameof(data));

            return _orchestrationContext.CreateSubOrchestrationInstance<(Dictionary<string, object>, Exception)>("statechart", invokeId, (invokeId, data));
        }

        internal override Task ProcessChildStateChartDoneAsync(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                if (! _childInstances.Remove(message.CorrelationId))
                {
                    Debug.Fail("Expected to find child state machine instance: " + message.CorrelationId);
                }
            }

            return Task.CompletedTask;
        }
    }
}
