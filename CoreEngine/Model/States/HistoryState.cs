using CoreEngine.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.States
{
    internal class HistoryState : State
    {
        private readonly HistoryType _type;

        public HistoryState(XElement element, State parent)
            : base(element, parent)
        {
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

        public override void InitDatamodel(ExecutionContext context, bool recursive)
        {
        }

        public void VisitTransition(List<State> targetStates,
                                    Dictionary<string, OrderedSet<ExecutableContent>> defaultHistoryContent,
                                    RootState root)
        {
            var transition = this._transitions.Value.Single();

            transition.StoreDefaultHistoryContent(_parent.Id, defaultHistoryContent);

            foreach (var targetState in transition.GetTargetStates(root))
            {
                targetStates.Append(targetState);
            }
        }
    }

    internal enum HistoryType
    {
        Deep,
        Shallow
    }
}
