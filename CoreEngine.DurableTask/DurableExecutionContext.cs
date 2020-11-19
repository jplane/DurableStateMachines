using DurableTask.Core;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.DurableTask
{
    public class DurableExecutionContext : ExecutionContext
    {
        private readonly Action<string, ExecutionContext, Func<ExecutionContext, Task>> _ensureActivityRegistration;

        private OrchestrationContext _orchestrationContext;

        public DurableExecutionContext(IModelMetadata metadata,
                                       OrchestrationContext orchestrationContext,
                                       Action<string, ExecutionContext, Func<ExecutionContext, Task>> ensureActivityRegistration)
            : base(metadata)
        {
            orchestrationContext.CheckArgNull(nameof(orchestrationContext));
            ensureActivityRegistration.CheckArgNull(nameof(ensureActivityRegistration));

            _orchestrationContext = orchestrationContext;
            _ensureActivityRegistration = ensureActivityRegistration;
        }

        internal override async Task Init()
        {
            Debug.Assert(_orchestrationContext != null);

            this["_sessionid"] = (await _orchestrationContext.ScheduleTask<Guid>(typeof(GenerateGuidActivity), string.Empty)).ToString("D");

            this["_name"] = this.Root.Name;
        }

        internal override Task ExecuteContent(string uniqueId, Func<ExecutionContext, Task> func)
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

        internal override Task LogDebug(string message)
        {
            Debug.Assert(_orchestrationContext != null);

            return _orchestrationContext.ScheduleTask<bool>("logger", string.Empty, ("debug", message));
        }

        internal override Task LogInformation(string message)
        {
            Debug.Assert(_orchestrationContext != null);

            return _orchestrationContext.ScheduleTask<bool>("logger", string.Empty, ("information", message));
        }
    }
}
