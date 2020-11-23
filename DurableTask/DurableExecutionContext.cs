using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.DurableTask
{
    public class DurableExecutionContext : ExecutionContext
    {
        private readonly Action<string, ExecutionContext, Func<ExecutionContext, Task>> _ensureActivityRegistration;

        private OrchestrationContext _orchestrationContext;

        public DurableExecutionContext(IRootStateMetadata metadata,
                                       OrchestrationContext orchestrationContext,
                                       Action<string, ExecutionContext, Func<ExecutionContext, Task>> ensureActivityRegistration,
                                       ILogger logger = null)
            : base(metadata, logger)
        {
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            ensureActivityRegistration.CheckArgNull(nameof(ensureActivityRegistration));

            _orchestrationContext = orchestrationContext;
            _ensureActivityRegistration = ensureActivityRegistration;
        }

        internal IDictionary<string, object> GetData() => _data;

        internal override async Task InitAsync()
        {
            Debug.Assert(_orchestrationContext != null);

            this["_sessionid"] = (await _orchestrationContext.ScheduleTask<Guid>(typeof(GenerateGuidActivity), string.Empty)).ToString("D");

            this["_name"] = this.Root.Name;
        }

        internal override Task ExecuteContentAsync(string uniqueId, Func<ExecutionContext, Task> func)
        {
            uniqueId.CheckArgNull(nameof(func));
            func.CheckArgNull(nameof(func));

            Debug.Assert(_orchestrationContext != null);

            _ensureActivityRegistration(uniqueId, this, func);

            return _orchestrationContext.ScheduleTask<bool>(uniqueId, string.Empty);
        }

        internal void EnqueueExternalMessage(Message message)
        {
            base.Send(message);
        }

        public override void Send(Message message)
        {
            message.CheckArgNull(nameof(message));

            Debug.Assert(_orchestrationContext != null);

            _orchestrationContext.SendEvent(_orchestrationContext.OrchestrationInstance, message.Name, message);
        }

        internal override Task LogDebugAsync(string message)
        {
            Debug.Assert(_orchestrationContext != null);

            if (_logger != null)
            {
                return _orchestrationContext.ScheduleTask<bool>("logger", string.Empty, ("debug", message));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        internal override Task LogInformationAsync(string message)
        {
            Debug.Assert(_orchestrationContext != null);

            if (_logger != null)
            {
                return _orchestrationContext.ScheduleTask<bool>("logger", string.Empty, ("information", message));
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}
