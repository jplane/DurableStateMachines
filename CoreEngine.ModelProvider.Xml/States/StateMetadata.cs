using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.ModelProvider.Xml.DataManipulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public abstract class StateMetadata : IStateMetadata
    {
        protected readonly XElement _element;

        protected StateMetadata(XElement element)
        {
            _element = element;
        }

        public virtual string Id => _element.Attribute("id")?.Value ?? string.Empty;

        public bool IsDescendentOf(IStateMetadata metadata)
        {
            return ((StateMetadata) metadata)._element.Descendants().Contains(this._element);
        }

        public int DepthFirstCompare(IStateMetadata metadata)
        {
            return XmlExtensions.GetDocumentOrder(_element, ((StateMetadata) metadata)._element);
        }

        public Task<IOnEntryExitMetadata> GetOnEntry()
        {
            var node = _element.ScxmlElement("onentry");

            return Task.FromResult(node == null ? null : (IOnEntryExitMetadata) new OnEntryExitMetadata(node));
        }

        public Task<IOnEntryExitMetadata> GetOnExit()
        {
            var node = _element.ScxmlElement("onexit");

            return Task.FromResult(node == null ? null : (IOnEntryExitMetadata) new OnEntryExitMetadata(node));
        }

        public Task<IEnumerable<ITransitionMetadata>> GetTransitions()
        {
            var nodes = _element.ScxmlElements("transition");

            return Task.FromResult(nodes.Select(n => new TransitionMetadata(n)).Cast<ITransitionMetadata>());
        }

        public Task<IEnumerable<IServiceMetadata>> GetServices()
        {
            var nodes = _element.ScxmlElements("invoke");

            return Task.FromResult(nodes.Select(n => new ServiceMetadata(n)).Cast<IServiceMetadata>());
        }

        public Task<IDatamodelMetadata> GetDatamodel()
        {
            var node = _element.ScxmlElement("datamodel");

            return Task.FromResult(node == null ? null : (IDatamodelMetadata) new DatamodelMetadata(node));
        }
    }
}
