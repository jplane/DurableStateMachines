using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Data;
using System;
using System.Threading.Tasks;
using System.Linq;
using StateChartsDotNet.Common;
using System.Threading;

namespace StateChartsDotNet.Model.States
{
    internal class FinalState : State
    {
        public FinalState(IFinalStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override bool IsFinalState => true;

        public override Task InvokeAsync(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }

        public override Task InitDatamodel(ExecutionContextBase context, bool recursive)
        {
            return Task.CompletedTask;
        }

        public Task SendDoneMessage(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IFinalStateMetadata) _metadata;

            var content = metadata.GetContent(context);

            var parameters = metadata.GetParams(context);

            return context.SendDoneMessageToParentAsync(content, parameters);
        }
    }
}
