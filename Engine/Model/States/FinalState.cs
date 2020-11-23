using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.DataManipulation;
using System;
using System.Threading.Tasks;
using System.Linq;

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
    }
}
