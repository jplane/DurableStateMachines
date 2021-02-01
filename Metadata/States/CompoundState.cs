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
    /// <see cref="CompoundState{TData}"/> is a <see cref="State"/> that contains one or more child states. When <see cref="CompoundState{TData}"/> is entered,
    ///  one of its child <see cref="States"/> is also entered; if that child is also a <see cref="CompoundState{TData}"/> or <see cref="ParallelState{TData}"/> then
    ///  the pattern continues recursively. For more information see: https://statecharts.github.io/glossary/compound-state.html
    /// It optionally defines actions that fire upon entry (<see cref="OnEntry"/>) and exit (<see cref="OnExit"/>).
    /// It optionally defines <see cref="Transitions"/> from this <see cref="AtomicState{TData}"/> to other states in the <see cref="StateMachine{TData}"/>.
    /// It defines child states in <see cref="States"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    public class CompoundState<TData> : State<TData>, IStateMetadata
    {
        private OnEntryExit<TData> _onEntry;
        private OnEntryExit<TData> _onExit;
        private MetadataList<Transition<TData>> _transitions;
        private MetadataList<State<TData>> _states;

        public CompoundState()
        {
            this.Transitions = new MetadataList<Transition<TData>>();
            this.States = new MetadataList<State<TData>>();
        }

        internal override int SetDocumentOrder(int order)
        {
            _documentOrder = order++;

            foreach (dynamic state in this.States)
            {
                order = state.SetDocumentOrder(order);
            }

            return order;
        }

        /// <summary>
        /// Initial child state to enter upon entrance to this <see cref="CompoundState{TData}"/>. If empty, the first child in <see cref="States"/> is used instead.
        /// </summary>
        [JsonProperty("initialstate")]
        public string InitialState { get; set; }

        /// <summary>
        /// Defines behavior that executes upon each entry into this <see cref="CompoundState{TData}"/>.
        /// </summary>
        [JsonProperty("onentry")]
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
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "compoundstate"}.onentry";
                    value.IsEntry = true;
                }

                _onEntry = value;
            }
        }

        /// <summary>
        /// Defines behavior that executes upon each exit from this <see cref="CompoundState{TData}"/>.
        /// </summary>
        [JsonProperty("onexit")]
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
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "compoundstate"}.onexit";
                    value.IsEntry = false;
                }

                _onExit = value;
            }
        }

        /// <summary>
        /// Defines transitions from this <see cref="CompoundState{TData}"/> to other <see cref="State"/>s.
        /// </summary>
        [JsonProperty("transitions")]
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "compoundstate"}.transitions";

                _transitions = value;
            }
        }

        /// <summary>
        /// The set of child states for this <see cref="CompoundState{TData}"/>.
        /// </summary>
        [JsonProperty("states")]
        public MetadataList<State<TData>> States
        {
            get => _states;

            private set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }

                if (_states != null)
                {
                    _states.MetadataIdResolver = null;
                }

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "compoundstate"}.states";
                value.ResolveDocumentOrder = this.ResolveDocumentOrder;

                _states = value;
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

            foreach (var state in this.States)
            {
                state.Validate(errorMap);
            }

            foreach (var transition in this.Transitions)
            {
                transition.Validate(errorMap);
            }

            this.OnEntry?.Validate(errorMap);

            this.OnExit?.Validate(errorMap);

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        StateType IStateMetadata.Type => StateType.Compound;

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

        IEnumerable<IInvokeStateMachineMetadata> IStateMetadata.GetStateMachineInvokes() => throw new NotSupportedException();

        ITransitionMetadata IStateMetadata.GetInitialTransition()
        {
            if (!string.IsNullOrWhiteSpace(this.InitialState))
            {
                return new Transition<TData>(this.InitialState, this.MetadataIdResolver(this));
            }
            else
            {
                var firstChild = ((IStateMetadata) this).GetStates().FirstOrDefault(sm => ! (sm is IHistoryStateMetadata));

                return firstChild == null ? null : new Transition<TData>(firstChild.Id, this.MetadataIdResolver(this));
            }
        }

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() =>
            this.States.Cast<IStateMetadata>() ?? Enumerable.Empty<IStateMetadata>();
    }
}
