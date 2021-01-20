using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model
{
    public interface IModelMetadata
    {
        string MetadataId { get; }
        IReadOnlyDictionary<string, object> DebuggerInfo { get; }
    }

    public static class ModelMetadataExtensions
    {
        public static void Validate(this IModelMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            if (string.IsNullOrWhiteSpace(metadata.MetadataId))
            {
                errors.Add(metadata, new List<string> { "MetadataId is invalid." });
            }
        }
    }
}
