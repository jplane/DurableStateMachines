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
    /// <summary>
    /// An action that assigns a value to a named location in execution state.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
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

        /// <summary>
        /// Target field or property in <typeparamref name="TData"/> for the assignment.
        /// </summary>
        public Expression<Func<TData, object>> To
        {
            set => _target = value.ExtractMember(nameof(To));
        }

        [JsonProperty("target")]
        private string TargetName { get; set; }

        /// <summary>
        /// Static value to assign to the target field or property.
        /// To derive this value at runtime using execution state <typeparamref name="TData"/>, use <see cref="ValueFunction"/>.
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <summary>
        /// Function to dynamically generate the assigned value at runtime, using execution state <typeparamref name="TData"/>.
        /// To use a static value, use <see cref="Value"/>.
        /// </summary>
        public Func<TData, object> ValueFunction { get; set; }

        [JsonProperty("valueexpression")]
        private string ValueExpression { get; set; }

        object IAssignMetadata.GetValue(dynamic data) => _valueGetter.Value(data);

        (string, MemberInfo) IAssignMetadata.Location => (this.TargetName, _target);
    }
}
