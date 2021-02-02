using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.States
{
    /// <summary>
    /// <see cref="FinalState{TData}"/> is a <see cref="State"/> that contains no child states. It indicates the end of execution
    ///  for its parent <see cref="CompoundState{TData}"/> or <see cref="StateMachine{TData}"/>.
    /// It optionally defines actions that fire upon entry (<see cref="OnEntry"/>) and exit (<see cref="OnExit"/>).
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "FinalState",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class FinalState<TData> : State<TData>, IFinalStateMetadata
    {
        private OnEntryExit<TData> _onEntry;
        private OnEntryExit<TData> _onExit;

        public FinalState()
        {
        }

        /// <summary>
        /// Defines behavior that executes upon each entry into this <see cref="FinalState{TData}"/>.
        /// </summary>
        [JsonProperty("onentry", Required = Required.DisallowNull)]
        public OnEntryExit<TData> OnEntry
        {
            get => _onEntry;

            set
            {
                if (_onEntry != null)
                {
                    _onEntry.MetadataIdResolver = null;
                }

                if (value != null)
                {
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "finalstate"}.onentry";
                    value.IsEntry = true;
                }

                _onEntry = value;
            }
        }

        /// <summary>
        /// Defines behavior that executes upon each exit from this <see cref="FinalState{TData}"/>.
        /// </summary>
        [JsonProperty("onexit", Required = Required.DisallowNull)]
        public OnEntryExit<TData> OnExit
        {
            get => _onExit;

            set
            {
                if (_onExit != null)
                {
                    _onExit.MetadataIdResolver = null;
                }

                if (value != null)
                {
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "finalstate"}.onexit";
                    value.IsEntry = false;
                }

                _onExit = value;
            }
        }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Id))
            {
                errors.Add("Id is invalid.");
            }

            this.OnEntry?.Validate(errorMap);

            this.OnExit?.Validate(errorMap);

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        StateType IStateMetadata.Type => StateType.Final;

        int IStateMetadata.GetDocumentOrder()
        {
            return this.GetDocumentOrder();
        }

        bool IStateMetadata.IsDescendentOf(IStateMetadata state)
        {
            return this.IsDescendentOf(state);
        }

        ITransitionMetadata IStateMetadata.GetInitialTransition() => throw new NotSupportedException();

        IOnEntryExitMetadata IStateMetadata.GetOnEntry() => this.OnEntry;

        IOnEntryExitMetadata IStateMetadata.GetOnExit() => this.OnExit;

        IEnumerable<IInvokeStateMachineMetadata> IStateMetadata.GetStateMachineInvokes() => throw new NotSupportedException();

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() => throw new NotSupportedException();

        IEnumerable<ITransitionMetadata> IStateMetadata.GetTransitions() => throw new NotSupportedException();
    }
}
