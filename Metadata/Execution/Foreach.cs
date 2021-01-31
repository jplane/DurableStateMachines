using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StateChartsDotNet.Metadata.Execution
{
    public class Foreach<TData> : ExecutableContent<TData>, IForeachMetadata
    {
        private MemberInfo _currentItemTarget;
        private MemberInfo _currentIndexTarget;
        private MetadataList<ExecutableContent<TData>> _actions;
        private readonly Lazy<Func<dynamic, IEnumerable>> _arrayGetter;

        public Foreach()
        {
            this.Actions = new MetadataList<ExecutableContent<TData>>();

            _arrayGetter = new Lazy<Func<dynamic, IEnumerable>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ValueExpression))
                {
                    return ExpressionCompiler.Compile<IEnumerable>(this.ValueExpression);
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

        [JsonProperty("actions")]
        public MetadataList<ExecutableContent<TData>> Actions
        {
            get => _actions;

            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_actions != null)
                {
                    _actions.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "foreach"}.actions";

                _actions = value;
            }
        }

        public Expression<Func<TData, object>> CurrentItemTarget
        {
            set => _currentItemTarget = value.ExtractMember(nameof(CurrentItemTarget));
        }

        [JsonProperty("currentitemlocation")]
        private string CurrentItemLocation { get; set; }

        public Expression<Func<TData, object>> CurrentIndexTarget
        {
            set => _currentIndexTarget = value.ExtractMember(nameof(CurrentIndexTarget));
        }

        [JsonProperty("currentindexlocation")]
        private string CurrentIndexLocation { get; set; }

        [JsonProperty("value")]
        public IEnumerable Value { get; set; }

        public Func<TData, IEnumerable> ValueFunction { get; set; }

        [JsonProperty("valueexpression")]
        private string ValueExpression { get; set; }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.ValueExpression) &&
                this.ValueFunction == null &&
                this.Value == null)
            {
                errors.Add("One of Value, ValueExpression, or ValueFunction must be set.");
            }

            foreach (var action in this.Actions)
            {
                action.Validate(errorMap);
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        IEnumerable IForeachMetadata.GetArray(dynamic data) => _arrayGetter.Value(data);

        (string, MemberInfo) IForeachMetadata.Item => (this.CurrentItemLocation, _currentItemTarget);

        (string, MemberInfo) IForeachMetadata.Index => (this.CurrentIndexLocation, _currentIndexTarget);

        IEnumerable<IExecutableContentMetadata> IForeachMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();
    }
}
