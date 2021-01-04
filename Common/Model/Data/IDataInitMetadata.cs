using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Data
{
    public interface IDataInitMetadata : IModelMetadata
    {
        string Id { get; }
        object GetValue(dynamic data);
    }

    public static class DataInitMetadataExtensions
    {
        public static void Validate(this IDataInitMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if (string.IsNullOrWhiteSpace(metadata.Id))
            {
                errors.Add(metadata, new List<string> { "Data init action requires a valid Id." });
            }
        }
    }
}
