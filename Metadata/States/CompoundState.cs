using Newtonsoft.Json;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateChartsDotNet.Metadata.States
{
    public class CompoundState : State, IStateMetadata
    {
        private OnEntryExit _onEntry;
        private OnEntryExit _onExit;
        private MetadataList<Transition> _transitions;
        private MetadataList<State> _states;

        public CompoundState()
        {
            this.Transitions = new MetadataList<Transition>();
            this.States = new MetadataList<State>();
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

        [JsonProperty("initialstate")]
        public string InitialState { get; set; }

        [JsonProperty("onentry")]
        public OnEntryExit OnEntry
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

        [JsonProperty("onexit")]
        public OnEntryExit OnExit
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

        [JsonProperty("transitions")]
        public MetadataList<Transition> Transitions
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

        [JsonProperty("states", ItemConverterType = typeof(StateConverter))]
        public MetadataList<State> States
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

        IEnumerable<IInvokeStateChartMetadata> IStateMetadata.GetStateChartInvokes() => throw new NotSupportedException();

        ITransitionMetadata IStateMetadata.GetInitialTransition()
        {
            if (!string.IsNullOrWhiteSpace(this.InitialState))
            {
                return new Transition(this.InitialState, this.MetadataIdResolver(this));
            }
            else
            {
                var firstChild = ((IStateMetadata) this).GetStates().FirstOrDefault(sm => ! (sm is IHistoryStateMetadata));

                return firstChild == null ? null : new Transition(firstChild.Id, this.MetadataIdResolver(this));
            }
        }

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() =>
            this.States.Cast<IStateMetadata>() ?? Enumerable.Empty<IStateMetadata>();
    }
}
