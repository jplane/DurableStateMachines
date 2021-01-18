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
    public class StateMetadata : IStateMetadata
    {
        protected readonly JObject _element;

        private readonly string _id;
        private readonly string _metadataId;

        internal StateMetadata(JObject element)
        {
            _element = element;

            _id = _element.Value<string>("id");
            _metadataId = element.GetUniqueElementPath();
        }

        public virtual string Id => _id ?? _metadataId;

        public virtual string MetadataId => _metadataId;

        public JObject DebuggerInfo
        {
            get
            {
                var json = new JObject();

                json["id"] = this.Id;
                json["metadataId"] = this.MetadataId;

                return json;
            }
        }

        public virtual StateType Type =>
            this.GetStates().Any() ? StateType.Compound : StateType.Atomic;

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

        public virtual ITransitionMetadata GetInitialTransition()
        {
            var attr = _element.Property("initial");

            if (attr != null)
            {
                return new TransitionMetadata(attr, this.MetadataId);
            }
            else
            {
                var firstChild = this.GetStates().FirstOrDefault(sm => ! (sm is IHistoryStateMetadata));

                return firstChild == null ? null : new TransitionMetadata(firstChild.Id, this.MetadataId);
            }
        }

        public virtual IEnumerable<IStateMetadata> GetStates()
        {
            var elements = _element.Property("states")?.Value?.Values<JObject>() ?? Enumerable.Empty<JObject>();

            var states = new List<IStateMetadata>();

            foreach (var el in elements)
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
