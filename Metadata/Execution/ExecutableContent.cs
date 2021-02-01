using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;

namespace DSM.Metadata.Execution
{
    public class ExecutableContent<TData> : IExecutableContentMetadata
    {
        protected internal ExecutableContent()
        {
        }

        [JsonProperty("type")]
        private string SerializationType
        {
            get => this.GetType().Name.ToLowerInvariant();
            set { }
        }

        internal virtual void Validate(IDictionary<string, List<string>> errors)
        {
        }

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["metadataId"] = ((IModelMetadata) this).MetadataId;

                return info;
            }
        }

        string IModelMetadata.MetadataId => this.MetadataIdResolver?.Invoke(this);

        protected internal Func<IModelMetadata, string> MetadataIdResolver { get; set; }
    }
}
