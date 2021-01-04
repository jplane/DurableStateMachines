using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IRaiseMetadata : IExecutableContentMetadata
    {
        string MessageName { get; }
    }

    public static class RaiseMetadataExtensions
    {
        public static void Validate(this IRaiseMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if (string.IsNullOrWhiteSpace(metadata.MessageName))
            {
                errors.Add(metadata, new List<string> { "Raise action requires message name." });
            }
        }
    }
}
