using Newtonsoft.Json;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DSM.Metadata.Execution
{
    /// <summary>
    /// An iterator over a collection of items.
    /// The child actions in <see cref="Actions"/> execute for each item in <see cref="Value"/> or <see cref="ValueFunction"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "Foreach",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class Foreach<TData> : Action<TData>, IForeachMetadata
    {
        private MemberInfo _currentItemTarget;
        private MemberInfo _currentIndexTarget;
        private MetadataList<Action<TData>> _actions;
        private readonly Lazy<Func<dynamic, IEnumerable>> _arrayGetter;

        public Foreach()
        {
            this.Actions = new MetadataList<Action<TData>>();

            _arrayGetter = new Lazy<Func<dynamic, IEnumerable>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ValueExpression))
                {
                    return ExpressionCompiler.Compile<IEnumerable>(((IModelMetadata)this).MetadataId, this.ValueExpression);
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

        /// <summary>
        /// The set of actions executed for this <see cref="Foreach{TData}"/> element.
        /// </summary>
        [JsonProperty("actions", ItemConverterType = typeof(ActionConverter), Required = Required.Always)]
        public MetadataList<Action<TData>> Actions
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

        /// <summary>
        /// Target field or property in <typeparamref name="TData"/> where the current item is assigned during execution.
        /// Reference this element in <see cref="Actions"/> elements as needed.
        /// </summary>
        [JsonIgnore]
        public Expression<Func<TData, object>> CurrentItem
        {
            set => _currentItemTarget = value.ExtractMember(nameof(CurrentItem));
        }

        [JsonProperty("currentitemlocation", Required = Required.DisallowNull)]
        private string CurrentItemLocation { get; set; }

        /// <summary>
        /// Target field or property in <typeparamref name="TData"/> where the current item index is assigned during execution.
        /// Reference this element in <see cref="Actions"/> elements as needed.
        /// </summary>
        [JsonIgnore]
        public Expression<Func<TData, int>> CurrentIndex
        {
            set => _currentIndexTarget = value.ExtractMember(nameof(CurrentIndex));
        }

        [JsonProperty("currentindexlocation", Required = Required.DisallowNull)]
        private string CurrentIndexLocation { get; set; }

        /// <summary>
        /// Static value to iterate over.
        /// To derive this value at runtime using execution state <typeparamref name="TData"/>, use <see cref="ValueFunction"/>.
        /// </summary>
        [JsonProperty("value", Required = Required.DisallowNull)]
        public IEnumerable Value { get; set; }

        /// <summary>
        /// Function to dynamically generate the iterated elements at runtime, using execution state <typeparamref name="TData"/>.
        /// To use a static value, use <see cref="Value"/>.
        /// </summary>
        [JsonIgnore]
        public Func<TData, IEnumerable> ValueFunction { get; set; }

        [JsonProperty("valueexpression", Required = Required.DisallowNull)]
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

        IEnumerable<IActionMetadata> IForeachMetadata.GetActions() =>
            this.Actions ?? Enumerable.Empty<IActionMetadata>();
    }
}
