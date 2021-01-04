using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface ICancelMetadata : IExecutableContentMetadata
    {
        string SendId { get; }
        string SendIdExpr { get; }
    }

    public static class CancelMetadataExtensions
    {
        public static void Validate(this ICancelMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if ((string.IsNullOrWhiteSpace(metadata.SendId) && string.IsNullOrWhiteSpace(metadata.SendIdExpr)) ||
                (!string.IsNullOrWhiteSpace(metadata.SendId) && !string.IsNullOrWhiteSpace(metadata.SendIdExpr)))
            {
                errors.Add(metadata, new List<string> { "Cancel action requires one of SendId or SendId expression." });
            }
        }
    }
}
