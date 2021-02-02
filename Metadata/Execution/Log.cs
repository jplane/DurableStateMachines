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
    /// An action that logs messages to a configured <see cref="ILogger"/> implementation.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    public sealed class Log<TData> : ExecutableContent<TData>, ILogMetadata
    {
        private Lazy<Func<dynamic, string>> _messageGetter;

        public Log()
        {
            _messageGetter = new Lazy<Func<dynamic, string>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.MessageExpression))
                {
                    return ExpressionCompiler.Compile<string>(this.MessageExpression);
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
        /// Static message to log.
        /// To derive this value at runtime using execution state <typeparamref name="TData"/>, use <see cref="MessageFunction"/>.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Function to dynamically generate the logged message at runtime, using execution state <typeparamref name="TData"/>.
        /// To use a static value, use <see cref="Message"/>.
        /// </summary>
        public Func<TData, string> MessageFunction { get; set; }

        [JsonProperty("messageexpression")]
        private string MessageExpression { get; set; }

        string ILogMetadata.GetMessage(dynamic data) => _messageGetter.Value(data);

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
