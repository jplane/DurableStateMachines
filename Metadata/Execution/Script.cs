using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateChartsDotNet.Metadata.Execution
{
    public class Script<TData> : ExecutableContent<TData>, IScriptMetadata
    {
        private Lazy<Func<dynamic, object>> _executor;

        public Script()
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

        public Action<TData> Function { get; set; }

        [JsonProperty("expression")]
        private string Expression { get; set; }

        public void Execute(dynamic data) => _executor.Value(data);

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
