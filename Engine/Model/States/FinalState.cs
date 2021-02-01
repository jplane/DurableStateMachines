using DSM.Common.Model.States;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using DSM.Engine;

namespace DSM.Engine.Model.States
{
    internal class FinalState : State
    {
        public FinalState(IFinalStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override Task InvokeAsync(ExecutionContextBase context)
        {
            return Task.CompletedTask;
        }

        public override IEnumerable<State> GetChildStates()
        {
            return Enumerable.Empty<State>();
        }

        public override void RecordHistory(ExecutionContextBase context)
        {
        }

        public override Transition GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }
    }
}
