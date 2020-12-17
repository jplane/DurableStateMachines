using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Metadata.Json.States
{
    public class ParallelStateMetadata : StateMetadata, IParallelStateMetadata
    {
        internal ParallelStateMetadata(JObject element)
            : base(element)
        {
        }

        public IEnumerable<IStateMetadata> GetStates()
        {
            var node = _element.Property("states");

            var states = new List<IStateMetadata>();

            foreach (var el in node.Value.Values<JObject>())
            {
                var type = el.Property("type")?.Value.Value<string>();

                if (type == null)
                {
                    states.Add(new AtomicStateMetadata(el));
                }
                else if (type == "sequential")
                {
                    states.Add(new SequentialStateMetadata(el));
                }
                else if (type == "parallel")
                {
                    states.Add(new ParallelStateMetadata(el));
                }
                else if (type == "history")
                {
                    states.Add(new HistoryStateMetadata(el));
                }
            }

            return states.AsEnumerable();
        }
    }
}
