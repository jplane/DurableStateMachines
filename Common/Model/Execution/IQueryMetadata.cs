using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IQueryMetadata : IExecutableContentMetadata
    {
        string ResultLocation { get; }

        string GetType(dynamic data);
        string GetTarget(dynamic data);
        IReadOnlyDictionary<string, object> GetParams(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }

    public static class QueryMetadataExtensions
    {
        public static void Validate(this IQueryMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if (string.IsNullOrWhiteSpace(metadata.ResultLocation))
            {
                errors.Add(metadata, new List<string> { "Query action requires result location." });
            }

            foreach (var executableContent in metadata.GetExecutableContent())
            {
                executableContent.Validate(errors);
            }
        }
    }
}
