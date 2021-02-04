using Newtonsoft.Json;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.Execution
{
    /// <summary>
    /// An action that raises an internal event in the state machine event queue.
    /// This can be used to discretely define state transitions, based on the event name or contents.
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "Raise",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class Raise<TData> : ExecutableContent<TData>, IRaiseMetadata
    {
        private Lazy<Func<dynamic, string>> _messageGetter;

        public Raise()
        {
            _messageGetter = new Lazy<Func<dynamic, string>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.MessageExpression))
                {
                    return ExpressionCompiler.Compile<string>(((IModelMetadata)this).MetadataId, this.MessageExpression);
                }
                else if (this.MessageFunction != null)
                {
                    return data => this.MessageFunction((TData) data);
                }
                else
                {
                    return _ => this.Message;
                }
            });
        }

        /// <summary>
        /// Static message to raise in the event.
        /// To derive this value at runtime using execution state <typeparamref name="TData"/>, use <see cref="MessageFunction"/>.
        /// </summary>
        [JsonProperty("message", Required = Required.DisallowNull)]
        public string Message { get; set; }

        /// <summary>
        /// Function to dynamically generate the logged message at runtime, using execution state <typeparamref name="TData"/>.
        /// To use a static value, use <see cref="Message"/>.
        /// </summary>
        [JsonIgnore]
        public Func<TData, string> MessageFunction { get; set; }

        [JsonProperty("messageexpression", Required = Required.DisallowNull)]
        private string MessageExpression { get; set; }

        string IRaiseMetadata.GetMessage(dynamic data) => _messageGetter.Value(data);

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.MessageExpression) &&
                this.MessageFunction == null &&
                this.Message == null)
            {
                errors.Add("One of Message, MessageExpression, or MessageFunction must be set.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }
    }
}
