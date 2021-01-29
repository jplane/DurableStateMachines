using StateChartsDotNet.Common.Model.States;
using System;
using System.Threading.Tasks;
using System.Linq;
using StateChartsDotNet.Common;
using System.Threading;
using System.Collections.Generic;

namespace StateChartsDotNet.Model.States
{
    internal class FinalState<TData> : State<TData>
    {
        public FinalState(IFinalStateMetadata metadata, State<TData> parent)
            : base(metadata, parent)
        {
        }

        public override Task InvokeAsync(ExecutionContextBase<TData> context)
        {
            return Task.CompletedTask;
        }

        public override IEnumerable<State<TData>> GetChildStates()
        {
            return Enumerable.Empty<State<TData>>();
        }

        public override void RecordHistory(ExecutionContextBase<TData> context)
        {
        }

        public override Transition<TData> GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }
    }
}
