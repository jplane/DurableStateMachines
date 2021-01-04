using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface ISendMessageMetadata : IExecutableContentMetadata
    {
        string Id { get; }
        string IdLocation { get; }

        string GetType(dynamic data);
        TimeSpan GetDelay(dynamic data);
        string GetTarget(dynamic data);
        string GetMessageName(dynamic data);
        object GetContent(dynamic data);
        IReadOnlyDictionary<string, object> GetParams(dynamic data);
    }

    public static class SendMessageMetadataExtensions
    {
        public static void Validate(this ISendMessageMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            if ((string.IsNullOrWhiteSpace(metadata.Id) && string.IsNullOrWhiteSpace(metadata.IdLocation)) ||
                (!string.IsNullOrWhiteSpace(metadata.Id) && !string.IsNullOrWhiteSpace(metadata.IdLocation)))
            {
                errors.Add(metadata, new List<string> { "Send Message action requires one of Id or Id location." });
            }
        }
    }
}
