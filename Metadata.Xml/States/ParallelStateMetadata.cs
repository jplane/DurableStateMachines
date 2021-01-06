using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class ParallelStateMetadata : StateMetadata
    {
        internal ParallelStateMetadata(XElement element)
            : base(element)
        {
        }

        public override StateType Type => StateType.Parallel;

        public override IEnumerable<IStateMetadata> GetStates()
        {
            var states = new List<IStateMetadata>();

            foreach (var el in _element.Elements())
            {
                if (el.ScxmlNameEquals("parallel"))
                {
                    states.Add(new ParallelStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("history"))
                {
                    states.Add(new HistoryStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("state"))
                {
                    states.Add(new StateMetadata(el));
                }
            }

            return states;
        }
    }
}
