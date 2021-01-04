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
    public abstract class StateMetadata : IStateMetadata
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
    }
}
