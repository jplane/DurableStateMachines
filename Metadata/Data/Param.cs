using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.ExpressionTrees;
using StateChartsDotNet.Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Data
{
    public class Param
    {
        private Lazy<Func<dynamic, object>> _valueGetter;

        public Param()
        {
            _valueGetter = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.Location))
                {
                    return data => data[this.Location];
                }
                else if (!string.IsNullOrWhiteSpace(this.ValueExpression))
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

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("valuefunction", ItemConverterType = typeof(ExpressionTreeConverter))]
        public Expression<Func<IDictionary<string, object>, object>> ValueFunction { get; set; }

        [JsonProperty("valueexpression")]
        public string ValueExpression { get; set; }

        public object GetValue(dynamic data) => _valueGetter.Value(data);

        internal void Validate(string metadataId, IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(metadataId));
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add("Parameter name is invalid.");
            }

            if (errors.Any())
            {
                errorMap.Add(metadataId, errors);
            }
        }
    }
}
