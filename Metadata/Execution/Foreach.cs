using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.ExpressionTrees;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace StateChartsDotNet.Metadata.Execution
{
    public class Foreach<TData> : ExecutableContent<TData>, IForeachMetadata
    {
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
                    return data => this.ValueFunction(data);
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

        [JsonProperty("currentitemlocation")]
        public string CurrentItemLocation { get; set; }

        [JsonProperty("currentindexlocation")]
        public string CurrentIndexLocation { get; set; }

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

            if (string.IsNullOrWhiteSpace(this.CurrentItemLocation))
            {
                errors.Add("CurrentItemLocation is invalid.");
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

        string IForeachMetadata.Item => this.CurrentItemLocation;

        string IForeachMetadata.Index => this.CurrentIndexLocation;

        IEnumerable<IExecutableContentMetadata> IForeachMetadata.GetExecutableContent() =>
            this.Actions ?? Enumerable.Empty<IExecutableContentMetadata>();
    }
}
