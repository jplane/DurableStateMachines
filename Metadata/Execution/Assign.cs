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
    public class Assign<TData> : ExecutableContent<TData>, IAssignMetadata
    {
        private Lazy<Func<dynamic, object>> _valueGetter;

        public Assign()
        {
            _valueGetter = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ValueExpression))
                {
                    return ExpressionCompiler.Compile<object>(this.ValueExpression);
                }
                else if (this.ValueFunction != null)
                {
                    return data => this.ValueFunction(data);
                }
                else
                {
                    return _ => this.Value;
                }
            });
        }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Location))
            {
                errors.Add("Location is invalid.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        public Func<TData, object> ValueFunction { get; set; }

        [JsonProperty("valueexpression")]
        private string ValueExpression { get; set; }

        public object GetValue(dynamic data) => _valueGetter.Value(data);
    }
}
