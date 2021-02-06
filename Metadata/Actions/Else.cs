using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.Actions
{
    /// <summary>
    /// The condition-less 'else' branch of an if-elseif-else control flow block.
    /// Only used in conjuction with <see cref="If{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "Else",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class Else<TData> : Action<TData>, IElseMetadata
    {
        private MetadataList<Action<TData>> _actions;

        public Else()
        {
            this.Actions = new MetadataList<Action<TData>>();
        }

        /// <summary>
        /// The set of actions executed for this <see cref="Else{TData}"/> branch.
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "else"}.actions";

                _actions = value;
            }
        }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            foreach (var action in this.Actions)
            {
                action.Validate(errorMap);
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        IEnumerable<IActionMetadata> IElseMetadata.GetActions() =>
            this.Actions ?? Enumerable.Empty<IActionMetadata>();
    }
}
