using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class StateMetadata : IStateMetadata
    {
        protected readonly XElement _element;

        private readonly string _id;
        private readonly string _metadataId;

        internal StateMetadata(XElement element)
        {
            _element = element;

            _id = _element.Attribute("id")?.Value;
            _metadataId = element.GetUniqueElementPath();
        }

        public virtual string Id => _id ?? _metadataId;

        public virtual string MetadataId => _metadataId;

        public virtual StateType Type =>
            this.GetStates().Any() ? StateType.Compound : StateType.Atomic;

        public bool IsDescendentOf(IStateMetadata metadata)
        {
            return ((StateMetadata) metadata)._element.Descendants().Contains(this._element);
        }

        public int DepthFirstCompare(IStateMetadata metadata)
        {
            return XmlExtensions.GetDocumentOrder(_element, ((StateMetadata) metadata)._element);
        }

        public IOnEntryExitMetadata GetOnEntry()
        {
            var node = _element.ScxmlElement("onentry");

            return node == null ? null : (IOnEntryExitMetadata) new OnEntryExitMetadata(node);
        }

        public IOnEntryExitMetadata GetOnExit()
        {
            var node = _element.ScxmlElement("onexit");

            return node == null ? null : (IOnEntryExitMetadata) new OnEntryExitMetadata(node);
        }

        public IEnumerable<ITransitionMetadata> GetTransitions()
        {
            var nodes = _element.ScxmlElements("transition");

            return nodes.Select(n => new TransitionMetadata(n)).Cast<ITransitionMetadata>();
        }

        public IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes()
        {
            var nodes = _element.ScxmlElements("invoke");

            return nodes.Select(n => new InvokeStateChartMetadata(n)).Cast<IInvokeStateChartMetadata>();
        }

        public IDatamodelMetadata GetDatamodel()
        {
            var node = _element.ScxmlElement("datamodel");

            return node == null ? null : (IDatamodelMetadata) new DatamodelMetadata(node);
        }

        public virtual ITransitionMetadata GetInitialTransition()
        {
            var attr = _element.Attribute("initial");

            if (attr != null)
            {
                return new TransitionMetadata(attr, this.MetadataId);
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
                    var firstChild = this.GetStates().FirstOrDefault(sm => ! (sm is IHistoryStateMetadata));

                    return firstChild == null ? null : new TransitionMetadata(firstChild.Id, this.MetadataId);
                }
            }
        }

        public virtual IEnumerable<IStateMetadata> GetStates()
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
                else if (el.ScxmlNameEquals("final"))
                {
                    states.Add(new FinalStateMetadata(el));
                }
            }

            return states.AsEnumerable();
        }
    }
}
