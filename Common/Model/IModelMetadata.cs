using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DSM.Common.Model
{
    public interface IModelMetadata
    {
        string MetadataId { get; }
        IReadOnlyDictionary<string, object> DebuggerInfo { get; }
    }
}
