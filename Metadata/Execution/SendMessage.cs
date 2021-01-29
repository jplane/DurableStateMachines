using Newtonsoft.Json;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateChartsDotNet.Metadata.Execution
{
    public class SendMessage<TData> : ExecutableContent<TData>, ISendMessageMetadata
    {
        public SendMessage()
        {
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("idlocation")]
        public string IdLocation { get; set; }

        [JsonProperty("delay")]
        public TimeSpan? Delay { get; set; }

        [JsonProperty("activitytype")]
        public string ActivityType { get; set; }

        [JsonProperty("configuration")]
        public ISendMessageConfiguration Configuration { get; set; }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (this.Configuration == null)
            {
                errors.Add("Configuration is invalid.");
            }

            if (string.IsNullOrWhiteSpace(this.ActivityType))
            {
                errors.Add("ActivityType is invalid.");
            }

            if (string.IsNullOrWhiteSpace(this.Id) && string.IsNullOrWhiteSpace(this.IdLocation))
            {
                errors.Add("One of Id or IdLocation must be set.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }
    }
}
