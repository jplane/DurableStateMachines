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
    /// <see cref="ParallelState{TData}"/> is a <see cref="State"/> that contains one or more child states. When <see cref="ParallelState{TData}"/> is entered,
    ///  each of its child <see cref="States"/> is also entered; if that child is also a <see cref="CompoundState{TData}"/> or <see cref="ParallelState{TData}"/> then
    ///  the pattern continues recursively. For more information see: https://statecharts.github.io/glossary/parallel-state.html
    /// It optionally defines actions that fire upon entry (<see cref="OnEntry"/>) and exit (<see cref="OnExit"/>).
    /// It optionally defines <see cref="Transitions"/> from this <see cref="AtomicState{TData}"/> to other states in the <see cref="StateMachine{TData}"/>.
    /// It defines child states in <see cref="States"/>.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    public class ParallelState<TData> : State<TData>, IStateMetadata
    {
        private OnEntryExit<TData> _onEntry;
        private OnEntryExit<TData> _onExit;
        private MetadataList<Transition<TData>> _transitions;
        private MetadataList<State<TData>> _states;

        public ParallelState()
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
        /// Defines behavior that executes upon each entry into this <see cref="ParallelState{TData}"/>.
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
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "parallelstate"}.onentry";
                    value.IsEntry = true;
                }

                _onEntry = value;
            }
        }

        /// <summary>
        /// Defines behavior that executes upon each exit from this <see cref="ParallelState{TData}"/>.
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
                    value.MetadataIdResolver = _ => $"{this.MetadataIdResolver?.Invoke(this) ?? "parallelstate"}.onexit";
                    value.IsEntry = false;
                }

                _onExit = value;
            }
        }

        /// <summary>
        /// Defines transitions from this <see cref="ParallelState{TData}"/> to other <see cref="State"/>s.
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "parallelstate"}.transitions";

                _transitions = value;
            }
        }

        /// <summary>
        /// The set of child states for this <see cref="ParallelState{TData}"/>.
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "parallelstate"}.states";
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
                if (state is FinalState<TData>)
                {
                    errors.Add("Parallel states cannot have FinalState child states.");
                }

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

        StateType IStateMetadata.Type => StateType.Parallel;

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

        ITransitionMetadata IStateMetadata.GetInitialTransition() => throw new NotSupportedException();

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() => this.States.Cast<IStateMetadata>() ?? Enumerable.Empty<IStateMetadata>();
    }
}
