using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.ExpressionTrees;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Execution
{
    public class Assign : ExecutableContent, IAssignMetadata
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
                    var func = this.ValueFunction.Compile();

                    Debug.Assert(func != null);

                    return data => func((IDictionary<string, object>)data);
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

        [JsonProperty("valuefunction", ItemConverterType = typeof(ExpressionTreeConverter))]
        public Expression<Func<IDictionary<string, object>, object>> ValueFunction { get; set; }

        [JsonProperty("valueexpression")]
        public string ValueExpression { get; set; }

        public object GetValue(dynamic data) => _valueGetter.Value(data);
    }
}
