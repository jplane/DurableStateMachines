using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Metadata.Json.States
{
    public class ParallelStateMetadata : StateMetadata
    {
        internal ParallelStateMetadata(JObject element)
            : base(element)
        {
        }

        public override StateType Type => StateType.Parallel;

        public override IEnumerable<IStateMetadata> GetStates()
        {
            var node = _element.Property("states");

            var states = new List<IStateMetadata>();

            foreach (var el in node.Value.Values<JObject>())
            {
                var type = el.Property("type")?.Value.Value<string>();

                if (type == null)
                {
                    states.Add(new StateMetadata(el));
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
