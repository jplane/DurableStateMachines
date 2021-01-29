using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.States
{
    internal class HistoryState<TData> : State<TData>
    {
        public HistoryState(IHistoryStateMetadata metadata, State<TData> parent)
            : base(metadata, parent)
        {
        }

        public bool IsDeep => ((IHistoryStateMetadata) _metadata).IsDeep;

        public override Task InvokeAsync(ExecutionContextBase<TData> context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<State<TData>> GetChildStates()
        {
            throw new NotImplementedException();
        }

        public override Transition<TData> GetInitialStateTransition()
        {
            throw new NotImplementedException();
        }

        public override void RecordHistory(ExecutionContextBase<TData> context)
        {
            throw new NotImplementedException();
        }

        public void VisitTransition(List<State<TData>> targetStates,
                                    Dictionary<string, Set<ExecutableContent<TData>>> defaultHistoryContent,
                                    StateChart<TData> root)
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
