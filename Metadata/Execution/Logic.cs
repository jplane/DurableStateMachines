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
    /// An action to define custom behavior during execution.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    public sealed class Logic<TData> : ExecutableContent<TData>, ILogicMetadata
    {
        private Lazy<Func<dynamic, object>> _executor;

        public Logic()
        {
            _executor = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.Expression))
                {
                    return ExpressionCompiler.Compile<object>(this.Expression);
                }
                else if (this.Function != null)
                {
                    return data =>
                    {
                        this.Function((TData) data);
                        return null;
                    };
                }
                else
                {
                    return _ => throw new InvalidOperationException("Unable to resolve script function or expression.");
                }
            });
        }

        /// <summary>
        /// Function that defines the core behavior of this <see cref="Logic{TData}"/> element.
        /// Execution state <typeparamref name="TData"/> is passed as an argument.
        /// </summary>
        public Action<TData> Function { get; set; }

        [JsonProperty("expression")]
        private string Expression { get; set; }

        void ILogicMetadata.Execute(dynamic data) => _executor.Value(data);

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Expression) && this.Function == null)
            {
                errors.Add("One of Expression or Function must be set.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }
    }
}
