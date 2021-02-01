using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.States;
using System;
using System.Collections.Generic;

namespace DSM.Metadata.States
{
    public class State<TData> : IModelMetadata
    {
        protected int? _documentOrder;

        protected internal State()
        {
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
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
