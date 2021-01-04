using System.Collections;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IForeachMetadata : IExecutableContentMetadata
    {
        string Item { get; }
        string Index { get; }

        IEnumerable GetArray(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }

    public static class ForeachMetadataExtensions
    {
        public static void Validate(this IForeachMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if (string.IsNullOrWhiteSpace(metadata.Item))
            {
                errors.Add(metadata, new List<string> { "Foreach action requires Item data location to store current item during processing." });
            }

            foreach (var executableContent in metadata.GetExecutableContent())
            {
                executableContent.Validate(errors);
            }
        }
    }
}
