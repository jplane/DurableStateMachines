﻿using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
{
    public class SequentialStateMetadata : StateMetadata, ISequentialStateMetadata
    {
        public SequentialStateMetadata(XElement element)
            : base(element)
        {
        }

        public async Task<ITransitionMetadata> GetInitialTransition()
        {
            var attr = _element.Attribute("initial");

            if (attr != null)
            {
                return new TransitionMetadata(attr);
            }
            else
            {
                var initialElement = _element.Element("initial");

                if (initialElement != null)
                {
                    var transitionElement = initialElement.ScxmlElement("transition");

                    return new TransitionMetadata(transitionElement);
                }
                else
                {
                    var firstChild = (await GetStates()).FirstOrDefault(sm => sm is IAtomicStateMetadata ||
                                                                              sm is ISequentialStateMetadata ||
                                                                              sm is IParallelStateMetadata ||
                                                                              sm is IFinalizeMetadata);

                    return firstChild == null ? null : new TransitionMetadata(firstChild.Id);
                }
            }
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
                else if (el.ScxmlNameEquals("final"))
                {
                    states.Add(new FinalStateMetadata(el));
                }
            }

            return Task.FromResult(states.AsEnumerable());
        }
    }
}
