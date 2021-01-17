using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IQueryMetadata : IExecutableContentMetadata
    {
        string ResultLocation { get; }
        string ActivityType { get; }
        JObject Config { get; }
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }

    public static class QueryMetadataExtensions
    {
        public static void Validate(this IQueryMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            var errs = new List<string>();

            if (!string.IsNullOrWhiteSpace(metadata.ActivityType))
            {
                errs.Add("Query action requires an activity type.");
            }

            if (metadata.Config == null)
            {
                errs.Add("Query action requires a configuration element.");
            }

            if (errs.Count > 0)
            {
                errors.Add(metadata, errs);
            }

            foreach (var executableContent in metadata.GetExecutableContent())
            {
                executableContent.Validate(errors);
            }
        }
    }
}
