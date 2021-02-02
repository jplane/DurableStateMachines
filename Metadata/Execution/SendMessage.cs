using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.Execution
{
    /// <summary>
    /// An action that models a one-way, fire-and-forget message as a Durable Functions Activity invocation.
    /// To use, implement <see cref="ISendMessageConfiguration"/> and an activity that accepts an instance of your custom type as input.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    public sealed class SendMessage<TData> : ExecutableContent<TData>, ISendMessageMetadata
    {
        public SendMessage()
        {
        }

        /// <summary>
        /// Identifier of this action instance. Can be used as a correlation identifier for messaging implementations that require it.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// An optional time delay for the messaging operation. If null, the operation occurs immediately.
        /// </summary>
        [JsonProperty("delay")]
        public TimeSpan? Delay { get; set; }

        /// <summary>
        /// Name of the custom activity that models your messaging operation.
        /// </summary>
        [JsonProperty("activitytype")]
        public string ActivityType { get; set; }

        /// <summary>
        /// An instance of your custom configuration that provides all needed information for the messaging operation.
        /// Instances of this class should be JSON-serializable.
        /// </summary>
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

            if (string.IsNullOrWhiteSpace(this.Id))
            {
                errors.Add("Id is invalid.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }
    }
}
