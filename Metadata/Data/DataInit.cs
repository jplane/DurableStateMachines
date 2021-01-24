using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.ExpressionTrees;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Data
{
    public class DataInit : IDataInitMetadata
    {
        private Lazy<Func<dynamic, object>> _valueGetter;

        public DataInit()
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

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("valuefunction", ItemConverterType = typeof(ExpressionTreeConverter))]
        public Expression<Func<IDictionary<string, object>, object>> ValueFunction { get; set; }

        [JsonProperty("valueexpression")]
        public string ValueExpression { get; set; }

        internal void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Id))
            {
                errors.Add("Id is invalid.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata) this).MetadataId, errors);
            }
        }

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["metadataId"] = ((IModelMetadata) this).MetadataId;

                return info;
            }
        }

        string IModelMetadata.MetadataId => this.MetadataIdResolver?.Invoke(this);

        internal Func<IModelMetadata, string> MetadataIdResolver { private get; set; }

        object IDataInitMetadata.GetValue(dynamic data) => _valueGetter.Value(data);
    }
}
