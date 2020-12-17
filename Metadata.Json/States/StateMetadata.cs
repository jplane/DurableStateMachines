using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Json.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Metadata.Json.States
{
    public abstract class StateMetadata : IStateMetadata
    {
        protected readonly JObject _element;

        private readonly string _id;
        private readonly Lazy<string> _uniqueId;

        internal StateMetadata(JObject element)
        {
            _element = element;

            _id = _element.Value<string>("id");

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public virtual string Id => _id ?? _uniqueId.Value;

        public virtual string UniqueId => _uniqueId.Value;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public bool IsDescendentOf(IStateMetadata metadata)
        {
            return ((StateMetadata) metadata)._element.Descendants().Contains(this._element);
        }

        public int DepthFirstCompare(IStateMetadata metadata)
        {
            return JsonExtensions.GetDocumentOrder(_element, ((StateMetadata) metadata)._element);
        }

        public IOnEntryExitMetadata GetOnEntry()
        {
            var node = _element.Property("onentry")?.Value.Value<JObject>();

            return node == null ? null : (IOnEntryExitMetadata) new OnEntryExitMetadata(node);
        }

        public IOnEntryExitMetadata GetOnExit()
        {
            var node = _element.Property("onexit")?.Value.Value<JObject>();

            return node == null ? null : (IOnEntryExitMetadata) new OnEntryExitMetadata(node);
        }

        public IEnumerable<ITransitionMetadata> GetTransitions()
        {
            var node = _element.Property("transitions");

            var nodes = Enumerable.Empty<ITransitionMetadata>();

            if (node != null)
            {
                nodes = node.Value.Values<JObject>().Select(jo => new TransitionMetadata(jo)).Cast<ITransitionMetadata>();
            }

            return nodes;
        }

        public IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes()
        {
            var node = _element.Property("invokes");

            var nodes = Enumerable.Empty<IInvokeStateChartMetadata>();

            if (node != null)
            {
                nodes = node.Value.Values<JObject>().Select(jo => new InvokeStateChartMetadata(jo)).Cast<IInvokeStateChartMetadata>();
            }

            return nodes;
        }

        public IDatamodelMetadata GetDatamodel()
        {
            var node = _element.Property("datamodel");

            return node == null ? null : (IDatamodelMetadata) new DatamodelMetadata(node);
        }
    }
}
