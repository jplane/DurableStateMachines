using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface ISendMessageMetadata : IExecutableContentMetadata
    {
        string Id { get; }
        string IdLocation { get; }
        TimeSpan Delay { get; }
        string ActivityType { get; }
        ISendMessageConfiguration Configuration { get; }
    }

    public static class SendMessageMetadataExtensions
    {
        public static void Validate(this ISendMessageMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            var errs = new List<string>();

            if ((string.IsNullOrWhiteSpace(metadata.Id) && string.IsNullOrWhiteSpace(metadata.IdLocation)) ||
                (!string.IsNullOrWhiteSpace(metadata.Id) && !string.IsNullOrWhiteSpace(metadata.IdLocation)))
            {
                errs.Add("Send message action requires one of Id or Id location.");
            }

            if (!string.IsNullOrWhiteSpace(metadata.ActivityType))
            {
                errs.Add("Send message action requires an activity type.");
            }

            if (metadata.Configuration == null)
            {
                errs.Add("Send message action requires a configuration element.");
            }

            if (errs.Count > 0)
            {
                errors.Add(metadata, errs);
            }
        }
    }
}
