using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Data;
using System;
using System.Threading.Tasks;
using System.Linq;
using StateChartsDotNet.Common;

namespace StateChartsDotNet.Model.States
{
    internal class FinalState : State
    {
        public FinalState(IFinalStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override bool IsFinalState => true;

        public override Task InvokeAsync(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override void InitDatamodel(ExecutionContext context, bool recursive)
        {
        }

        public void SendDoneMessage(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IFinalStateMetadata) _metadata;

            var content = metadata.GetContent(context);

            var parameters = metadata.GetParams(context);

            context.SendDoneMessageToParent(content, parameters);
        }
    }
}
