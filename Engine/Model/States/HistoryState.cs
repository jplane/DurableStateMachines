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

        public override bool IsHistoryState => true;

        public override bool IsDeepHistoryState => ((IHistoryStateMetadata) _metadata).Type == HistoryType.Deep;

        public override Task InvokeAsync(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override void InitDatamodel(ExecutionContext context, bool recursive)
        {
        }

        public void VisitTransition(List<State> targetStates,
                                    Dictionary<string, Set<ExecutableContent>> defaultHistoryContent,
                                    RootState root)
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
