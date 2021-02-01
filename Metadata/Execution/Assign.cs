using Newtonsoft.Json;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DSM.Metadata.Execution
{
    public class Assign<TData> : ExecutableContent<TData>, IAssignMetadata
    {
        private MemberInfo _target;
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
                    return data => this.ValueFunction((TData) data);
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

            if (string.IsNullOrWhiteSpace(this.TargetName) && _target == null)
            {
                errors.Add("One of TargetName or Target must be set.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        public Expression<Func<TData, object>> Target
        {
            set => _target = value.ExtractMember(nameof(Target));
        }

        [JsonProperty("target")]
        private string TargetName { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        public Func<TData, object> ValueFunction { get; set; }

        [JsonProperty("valueexpression")]
        private string ValueExpression { get; set; }

        public object GetValue(dynamic data) => _valueGetter.Value(data);

        (string, MemberInfo) IAssignMetadata.Location => (this.TargetName, _target);
    }
}
