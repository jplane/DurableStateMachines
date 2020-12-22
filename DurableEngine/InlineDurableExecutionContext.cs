using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class InlineDurableExecutionContext : DurableExecutionContext
    {
        public InlineDurableExecutionContext(IStateChartMetadata metadata,
                                             OrchestrationContext orchestrationContext,
                                             IDictionary<string, object> data,
                                             CancellationToken cancelToken,
                                             ILogger logger = null)
            : base(metadata, orchestrationContext, data, cancelToken, logger)
        {
        }

        protected override Task StartChildOrchestrationAsync(string uniqueId, string invokeId, Dictionary<string, object> data)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(uniqueId));
            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));
            Debug.Assert(data != null);

            Debug.Assert(_orchestrationContext != null);

            return _orchestrationContext.CreateSubOrchestrationInstance<(Dictionary<string, object>, Exception)>("statechart", uniqueId, invokeId, data);
        }

        internal override Task ProcessChildStateChartDoneAsync(ChildStateChartResponseMessage message)
        {
            message.CheckArgNull(nameof(message));

            if (message.IsDone)
            {
                foreach (var pair in _childInstances.ToArray())
                {
                    if (pair.Value.Remove(message.CorrelationId))
                    {
                        if (pair.Value.Count == 0)
                        {
                            _childInstances.Remove(pair.Key);
                        }

                        return Task.CompletedTask;
                    }
                }

                Debug.Fail("Expected to find child state machine instance: " + message.CorrelationId);
            }

            return Task.CompletedTask;
        }
    }
}
