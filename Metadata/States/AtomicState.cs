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
    /// <see cref="AtomicState{TData}"/> is a <see cref="State"/> that contains no child states.
    /// It optionally defines actions that fire upon entry (<see cref="OnEntry"/>) and exit (<see cref="OnExit"/>).
    /// It optionally defines <see cref="Transitions"/> from this <see cref="AtomicState{TData}"/> to other states in the <see cref="StateMachine{TData}"/>.
    /// It optionally defines invocations of child state machines in <see cref="Invokes"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "AtomicState",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class AtomicState<TData> : State<TData>, IStateMetadata
    {
        private OnEntryExit<TData> _onEntry;
        private OnEntryExit<TData> _onExit;
        private MetadataList<Transition<TData>> _transitions;
        private MetadataList<InvokeStateMachine<TData>> _invokes;

        public AtomicState()
        {
            this.Transitions = new MetadataList<Transition<TData>>();
            this.Invokes = new MetadataList<InvokeStateMachine<TData>>();
        }

        /// <summary>
        /// Defines behavior that executes upon each entry into this <see cref="AtomicState{TData}"/>.
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
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "atomicstate"}.onentry";
                    value.IsEntry = true;
                }

                _onEntry = value;
            }
        }

        /// <summary>
        /// Defines behavior that executes upon each exit from this <see cref="AtomicState{TData}"/>.
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
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "atomicstate"}.onexit";
                    value.IsEntry = false;
                }

                _onExit = value;
            }
        }

        /// <summary>
        /// Defines transitions from this <see cref="AtomicState{TData}"/> to other <see cref="State"/>s.
        /// </summary>
        [JsonProperty("transitions", Required = Required.DisallowNull)]
        public MetadataList<Transition<TData>> Transitions
        {
            get => _transitions;

            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_transitions != null)
                {
                    _transitions.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "atomicstate"}.transitions";

                _transitions = value;
            }
        }

        /// <summary>
        /// Defines child state machine invocations that occur upon entry into this <see cref="AtomicState{TData}"/>.
        /// </summary>
        [JsonProperty("childinvocations", Required = Required.DisallowNull)]
        public MetadataList<InvokeStateMachine<TData>> Invokes
        {
            get => _invokes;

            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_invokes != null)
                {
                    _invokes.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "atomicstate"}.invokes";

                _invokes = value;
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

            foreach (var transition in this.Transitions)
            {
                transition.Validate(errorMap);
            }

            foreach (var invoke in this.Invokes)
            {
                invoke.Validate(errorMap);
            }

            this.OnEntry?.Validate(errorMap);

            this.OnExit?.Validate(errorMap);

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        StateType IStateMetadata.Type => StateType.Atomic;

        bool IStateMetadata.IsDescendentOf(IStateMetadata state)
        {
            return this.IsDescendentOf(state);
        }

        int IStateMetadata.GetDocumentOrder()
        {
            return this.GetDocumentOrder();
        }

        IOnEntryExitMetadata IStateMetadata.GetOnEntry() => this.OnEntry;

        IOnEntryExitMetadata IStateMetadata.GetOnExit() => this.OnExit;

        IEnumerable<ITransitionMetadata> IStateMetadata.GetTransitions() =>
            this.Transitions ?? Enumerable.Empty<ITransitionMetadata>();

        IEnumerable<IInvokeStateMachineMetadata> IStateMetadata.GetStateMachineInvokes() =>
            this.Invokes ?? Enumerable.Empty<IInvokeStateMachineMetadata>();

        ITransitionMetadata IStateMetadata.GetInitialTransition() => throw new NotSupportedException();

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() => Enumerable.Empty<IStateMetadata>();
    }
}
