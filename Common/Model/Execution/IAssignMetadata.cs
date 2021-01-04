using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IAssignMetadata : IExecutableContentMetadata
    {
        string Location { get; }
        object GetValue(dynamic data);
    }

    public static class AssignMetadataExtensions
    {
        public static void Validate(this IAssignMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata)metadata).Validate(errors);

            if (string.IsNullOrWhiteSpace(metadata.Location))
            {
                errors.Add(metadata, new List<string> { "Assign action requires a data location." });
            }
        }
    }
}
