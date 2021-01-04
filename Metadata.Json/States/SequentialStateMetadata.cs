using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.States;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Metadata.Json.States
{
    public class SequentialStateMetadata : StateMetadata, ISequentialStateMetadata
    {
        internal SequentialStateMetadata(JObject element)
            : base(element)
        {
        }

        public ITransitionMetadata GetInitialTransition()
        {
            var attr = _element.Property("initial");

            if (attr != null)
            {
                return new TransitionMetadata(attr, this.MetadataId);
            }
            else
            {
                var firstChild = GetStates().FirstOrDefault(sm => sm is IAtomicStateMetadata ||
                                                                  sm is ISequentialStateMetadata ||
                                                                  sm is IParallelStateMetadata ||
                                                                  sm is IFinalStateMetadata);

                return firstChild == null ? null : new TransitionMetadata(firstChild.Id, this.MetadataId);
            }
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
                else if (type == "final")
                {
                    states.Add(new FinalStateMetadata(el));
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
