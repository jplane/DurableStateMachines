using Newtonsoft.Json;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StateChartsDotNet.Metadata.States
{
    public class StateMachine<TData> : IStateChartMetadata
    {
        private Script<TData> _initScript;
        private MetadataList<State<TData>> _states;

        public StateMachine()
        {
            this.States = new MetadataList<State<TData>>();
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("initialstate")]
        public virtual string InitialState { get; set; }

        [JsonProperty("failfast")]
        public bool FailFast { get; set; }

        [JsonProperty("initscript")]
        public Script<TData> InitScript
        {
            get => _initScript;

            set
            {
                if (_initScript != null)
                {
                    _initScript.MetadataIdResolver = null;
                }

                if (value != null)
                {
                    value.MetadataIdResolver = _ => $"{this.Id ?? "statemachine"}.initscript";
                }

                _initScript = value;
            }
        }

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

                value.MetadataIdResolver = () => $"{this.Id ?? "statemachine"}.states";
                value.ResolveDocumentOrder = this.SetDocumentOrder;

                _states = value;
            }
        }

        internal void SetDocumentOrder()
        {
            var order = 1;

            foreach (dynamic state in this.States)
            {
                order = state.SetDocumentOrder(order);
            }
        }

        public void Validate()
        {
            var errorMap = new Dictionary<string, List<string>>();

            Validate(errorMap);

            if (errorMap.Any())
            {
                throw new MetadataValidationException(errorMap.ToDictionary(p => p.Key, p => p.Value.ToArray()));
            }
        }

        internal void Validate(IDictionary<string, List<string>> errorMap)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Id))
            {
                errors.Add("Id is invalid.");
            }

            foreach (var state in this.States)
            {
                if (state is HistoryState<TData>)
                {
                    errors.Add("History states cannot be a direct child of a state machine root.");
                }

                state.Validate(errorMap);
            }

            this.InitScript?.Validate(errorMap);

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        string IModelMetadata.MetadataId => this.Id;

        StateType IStateMetadata.Type => StateType.Root;

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo
        {
            get
            {
                var info = new Dictionary<string, object>();

                info["id"] = this.Id;
                info["metadataId"] = ((IModelMetadata) this).MetadataId;

                return info;
            }
        }

        int IStateMetadata.GetDocumentOrder() => 0;

        bool IStateMetadata.IsDescendentOf(IStateMetadata state) => false;

        ITransitionMetadata IStateMetadata.GetInitialTransition()
        {
            if (!string.IsNullOrWhiteSpace(this.InitialState))
            {
                return new Transition<TData>(this.InitialState, this.Id);
            }
            else
            {
                var firstChild = ((IStateMetadata)this).GetStates().FirstOrDefault(sm => !(sm is IHistoryStateMetadata));

                return firstChild == null ? null : new Transition<TData>(firstChild.Id, this.Id);
            }
        }

        IScriptMetadata IStateChartMetadata.GetScript() => this.InitScript;

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() => this.States.Cast<IStateMetadata>();

        IOnEntryExitMetadata IStateMetadata.GetOnEntry() => throw new NotSupportedException();

        IOnEntryExitMetadata IStateMetadata.GetOnExit() => throw new NotSupportedException();

        IEnumerable<IInvokeStateChartMetadata> IStateMetadata.GetStateChartInvokes() => throw new NotSupportedException();

        IEnumerable<ITransitionMetadata> IStateMetadata.GetTransitions() => Enumerable.Empty<ITransitionMetadata>();
    }
}
