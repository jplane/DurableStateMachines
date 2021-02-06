using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DSM.Metadata.Actions
{
    /// <summary>
    /// An action that models a one-way, fire-and-forget message as a Durable Functions Activity invocation.
    /// To use, implement <see cref="ISendMessageConfiguration"/> and an activity that accepts an instance of your custom type as input.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "SendMessage",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class SendMessage<TData> : Action<TData>, ISendMessageMetadata
    {
        public SendMessage()
        {
        }

        /// <summary>
        /// Identifier of this action instance. Can be used as a correlation identifier for messaging implementations that require it.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// An optional time delay for the messaging operation. If null, the operation occurs immediately.
        /// </summary>
        [JsonProperty("delay", Required = Required.DisallowNull)]
        public TimeSpan? Delay { get; set; }

        /// <summary>
        /// Name of the custom activity that models your messaging operation.
        /// </summary>
        [JsonProperty("activitytype", Required = Required.Always)]
        public string ActivityType { get; set; }

        /// <summary>
        /// An instance of your custom configuration that provides all needed information for the messaging operation.
        /// Instances of this class should be JSON-serializable.
        /// </summary>
        public object Configuration { get; set; }

        [JsonProperty("configuration", Required = Required.Always)]
        private JObject JsonConfig { get; set; }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (this.Configuration == null && this.JsonConfig == null)
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

        (object, JObject) ISendMessageMetadata.GetConfiguration() => (this.Configuration, this.JsonConfig);
    }
}
