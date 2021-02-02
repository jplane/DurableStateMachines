using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.States;
using System;
using System.Collections.Generic;

namespace DSM.Metadata.States
{
    /// <summary>
    /// Represents a state in a state machine definition. Uniquely identified by <see cref="Id"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "State",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public abstract class State<TData> : IModelMetadata
    {
        protected int? _documentOrder;

        internal State()
        {
        }

        /// <summary>
        /// Unique identifier for this <see cref="State"/> within the state machine definition.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        private string SerializationType
        {
            get => ((IStateMetadata)this).Type.ToString().ToLowerInvariant();
            set { }
        }

        internal virtual void Validate(IDictionary<string, List<string>> errors)
        {
        }

        protected internal Func<IModelMetadata, string> MetadataIdResolver { protected get; set; }

        protected internal Action ResolveDocumentOrder { protected get; set; }

        internal virtual int SetDocumentOrder(int order)
        {
            _documentOrder = order++;
            return order;
        }

        internal bool IsDescendentOf(IStateMetadata state)
        {
            return ((IStateMetadata)this).MetadataId.StartsWith(state.MetadataId);
        }

        internal int GetDocumentOrder()
        {
            if (_documentOrder == null)
            {
                ResolveDocumentOrder();
            }

            return _documentOrder.Value;
        }

        string IModelMetadata.MetadataId => this.MetadataIdResolver?.Invoke(this);

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["id"] = this.Id;
                info["metadataId"] = ((IModelMetadata) this).MetadataId;

                return info;
            }
        }
    }
}
