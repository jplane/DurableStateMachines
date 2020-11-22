using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
{
    public abstract class StateMetadata : IStateMetadata
    {
        protected readonly XElement _element;

        protected StateMetadata(XElement element)
        {
            _element = element;
        }

        public string UniqueId => this.Id;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
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

            return nodes.Select(n => new InvokeStateChart(n)).Cast<IInvokeStateChartMetadata>();
        }

        public IDatamodelMetadata GetDatamodel()
        {
            var node = _element.ScxmlElement("datamodel");

            return node == null ? null : (IDatamodelMetadata) new DatamodelMetadata(node);
        }
    }
}
