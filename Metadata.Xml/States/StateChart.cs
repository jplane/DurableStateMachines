using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace StateChartsDotNet.Metadata.Xml.States
{
    public class StateChart : StateMetadata, IRootStateMetadata
    {
        public StateChart(XDocument document)
            : base(document.Root)
        {
        }

        protected override string IdAttributeName => "name";

        public bool FailFast
        {
            get => bool.Parse(_element.Attribute("failfast")?.Value ?? "false");
        }

        public Databinding Databinding
        {
            get => (Databinding) Enum.Parse(typeof(Databinding),
                                            _element.Attribute("binding")?.Value ?? "early",
                                            true);
        }

        public ITransitionMetadata GetInitialTransition()
        {
            var attr = _element.Attribute("initial");

            if (attr != null)
            {
                return new TransitionMetadata(attr);
            }
            else
            {
                var firstChild = GetStates().FirstOrDefault(sm => sm is IAtomicStateMetadata ||
                                                                  sm is ISequentialStateMetadata ||
                                                                  sm is IParallelStateMetadata ||
                                                                  sm is IFinalStateMetadata);

                return firstChild == null ? null : new TransitionMetadata(firstChild.Id);
            }
        }

        public IScriptMetadata GetScript()
        {
            var node = _element.ScxmlElement("script");

            return node == null ? null : (IScriptMetadata) new ScriptMetadata(node);
        }

        public IEnumerable<IStateMetadata> GetStates()
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
                else if (el.ScxmlNameEquals("final"))
                {
                    states.Add(new FinalStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("state"))
                {
                    states.Add(new AtomicStateMetadata(el));
                }
            }

            return states.AsEnumerable();
        }
    }
}
