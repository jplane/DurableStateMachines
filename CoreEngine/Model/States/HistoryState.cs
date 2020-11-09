using CoreEngine.Abstractions.Model;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.States
{
    internal class HistoryState : State
    {
        public HistoryState(IHistoryStateMetadata metadata, State parent)
            : base(metadata, parent)
        {
        }

        public override bool IsHistoryState => true;

        public override bool IsDeepHistoryState => ((IHistoryStateMetadata) _metadata).Type == HistoryType.Deep;

        public override Task Invoke(ExecutionContext context, RootState root)
        {
            throw new NotImplementedException();
        }

        public override Task InitDatamodel(ExecutionContext context, bool recursive)
        {
            return Task.CompletedTask;
        }

        public async Task VisitTransition(List<State> targetStates,
                                          Dictionary<string, Set<ExecutableContent>> defaultHistoryContent,
                                          RootState root)
        {
            var transition = (await _transitions).Single();

            await transition.StoreDefaultHistoryContent(_parent.Id, defaultHistoryContent);

            foreach (var targetState in await transition.GetTargetStates(root))
            {
                targetStates.Add(targetState);
            }
        }
    }
}
