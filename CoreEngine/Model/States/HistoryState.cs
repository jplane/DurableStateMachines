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
        private readonly HistoryType _type;

        public HistoryState(XElement element, State parent)
            : base(element, parent)
        {
            element.CheckArgNull(nameof(element));

            _type = (HistoryType) Enum.Parse(typeof(HistoryType),
                                             element.Attribute("type")?.Value ?? "shallow",
                                             true);
        }

        public override bool IsHistoryState => true;

        public override bool IsDeepHistoryState => _type == HistoryType.Deep;

        public override void Invoke(ExecutionContext context, RootState root)
        {
            throw new NotImplementedException();
        }

        public override Task InitDatamodel(ExecutionContext context, bool recursive)
        {
            return Task.CompletedTask;
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

    internal enum HistoryType
    {
        Deep,
        Shallow
    }
}
