using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Data;
using System;
using System.Threading.Tasks;
using System.Linq;
using StateChartsDotNet.Common;
using System.Threading;
using System.Collections.Generic;

namespace StateChartsDotNet.Model.States
{
    internal class FinalState : State
    {
        public FinalState(IFinalStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override Task InvokeAsync(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }

        public override Task InitDatamodel(ExecutionContextBase context, bool recursive)
        {
            return Task.CompletedTask;
        }

        public override IEnumerable<State> GetChildStates()
        {
            throw new NotImplementedException();
        }

        public override void RecordHistory(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }

        public override Transition GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }
    }
}
