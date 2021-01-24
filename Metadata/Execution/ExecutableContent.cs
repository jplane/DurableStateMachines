using Newtonsoft.Json;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Execution
{
    public abstract class ExecutableContent : IExecutableContentMetadata
    {
        internal ExecutableContent()
        {
        }

        [JsonProperty("type")]
        private string SerializationType
        {
            get => this.GetType().Name.ToLowerInvariant();
            set { }
        }

        internal abstract void Validate(IDictionary<string, List<string>> errors);

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
