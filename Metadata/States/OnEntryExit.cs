﻿using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.Actions;
using DSM.Common.Model.States;
using DSM.Metadata.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.States
{
    /// <summary>
    /// <see cref="OnEntryExit{TData}"/> models the actions that execute upon entry to or exit from a <see cref="State{TData}"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "OnEntryExit",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class OnEntryExit<TData> : IOnEntryExitMetadata
    {
        private MetadataList<Actions.Action<TData>> _actions;

        public OnEntryExit()
        {
            this.Actions = new MetadataList<Actions.Action<TData>>();
        }

        internal bool IsEntry { private get; set; }

        internal Func<IModelMetadata, string> MetadataIdResolver { private get; set; }

        string IModelMetadata.MetadataId => this.MetadataIdResolver?.Invoke(this);

        /// <summary>
        /// The set of actions for this <see cref="OnEntryExit{TData}"/>.
        /// </summary>
        [JsonProperty("actions", ItemConverterType = typeof(ActionConverter), Required = Required.Always)]
        public MetadataList<Actions.Action<TData>> Actions
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "onentryexit"}.actions";

                _actions = value;
            }
        }

        internal void Validate(IDictionary<string, List<string>> errorMap)
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

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["metadataId"] = ((IModelMetadata)this).MetadataId;

                return info;
            }
        }

        bool IOnEntryExitMetadata.IsEntry => this.IsEntry;

        IEnumerable<IActionMetadata> IOnEntryExitMetadata.GetActions() =>
            this.Actions ?? Enumerable.Empty<IActionMetadata>();
    }
}
