using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.States
{
    internal class HistoryState : State
    {
        public HistoryState(IHistoryStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public bool IsDeep => ((IHistoryStateMetadata) _metadata).IsDeep;

        public override Task InvokeAsync(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<State> GetChildStates()
        {
            throw new NotImplementedException();
        }

        public override Transition GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }

        public override void RecordHistory(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }

        public void VisitTransition(List<State> targetStates,
                                    Dictionary<string, Set<ExecutableContent>> defaultHistoryContent,
                                    StateChart root)
        {
            var transition = _transitions.Value.Single();

            transition.StoreDefaultHistoryContent(_parent.Id, defaultHistoryContent);

            foreach (var targetState in transition.GetTargetStates(root))
            {
                targetStates.Add(targetState);
            }
        }
    }
}
