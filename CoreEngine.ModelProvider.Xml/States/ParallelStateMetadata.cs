using CoreEngine.Abstractions.Model.States.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public class ParallelStateMetadata : StateMetadata, IParallelStateMetadata
    {
        public ParallelStateMetadata(XElement element)
            : base(element)
        {
        }

        public Task<IEnumerable<IStateMetadata>> GetStates()
        {
            var states = new List<IStateMetadata>();

            bool IsCompoundState(XElement el)
            {
                return el.ScxmlNameEquals("state") &&
                       el.Elements().Any(ce => ce.ScxmlNameIn("state", "parallel", "final"));
            }

            foreach (var el in _element.Elements())
            {
                if (IsCompoundState(el))
                {
                    states.Add(new SequentialStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("parallel"))
                {
                    states.Add(new ParallelStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("history"))
                {
                    states.Add(new HistoryStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("state"))
                {
                    states.Add(new AtomicStateMetadata(el));
                }
            }

            return Task.FromResult(states.AsEnumerable());
        }
    }
}
