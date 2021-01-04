using StateChartsDotNet.Common.Model.Execution;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IOnEntryExitMetadata : IModelMetadata
    {
        bool IsEntry { get; }

        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }

    public static class OnEntryExitMetadataExtensions
    {
        public static void Validate(this IOnEntryExitMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            foreach (var executableContent in metadata.GetExecutableContent())
            {
                executableContent.Validate(errors);
            }
        }
    }
}
