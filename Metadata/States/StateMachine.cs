using Newtonsoft.Json;
using DSM.Common.Exceptions;
using DSM.Common.Model;
using DSM.Common.Model.Actions;
using DSM.Common.Model.States;
using DSM.Metadata.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DSM.Metadata.States
{
    /// <summary>
    /// <see cref="StateMachine{TData}"/> defines the child states, transitions, and actions that together determine the state machines behavior and lifecycle.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "StateMachine",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class StateMachine<TData> : IStateMachineMetadata
    {
        private Logic<TData> _initScript;
        private MetadataList<State<TData>> _states;

        public StateMachine()
        {
            this.States = new MetadataList<State<TData>>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static StateMachine<TData> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<StateMachine<TData>>(json);
        }

        public static string GenerateSchema()
        {
            var generator = new JSchemaGenerator();

            generator.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var jschema = generator.Generate(typeof(StateMachine<TData>));

            return jschema.ToString();
        }

        /// <summary>
        /// Unique identifier for this <see cref="StateMachine{TData}"/>.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Initial child state to enter upon entrance to this <see cref="StateMachine{TData}"/>. If empty, the first child in <see cref="States"/> is used instead.
        /// </summary>
        [JsonProperty("initialstate")]
        public string InitialState { get; set; }

        /// <summary>
        /// Normal <see cref="StateMachine{TData}"/> error semantics result in an error event with message name 'error.[REASON]'; such events can be targeted
        ///  using <see cref="Transition{TData}.Message"/> to cause transitions to defined error <see cref="State{TData}"/>s.
        /// If <see cref="FailFast"/> = true, the state machine interpreter will instead cause execution to stop gracefully and prevent <see cref="Transition{TData}"/> handling
        ///  of errors.
        /// </summary>
        [JsonProperty("failfast", Required = Required.DisallowNull)]
        public bool FailFast { get; set; }

        /// <summary>
        /// Defines optional initialization logic for this <see cref="StateMachine{TData}"/>.
        /// </summary>
        [JsonProperty("initlogic", Required = Required.DisallowNull)]
        public Logic<TData> InitLogic
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

        /// <summary>
        /// The set of child states for this <see cref="StateMachine{TData}"/>.
        /// </summary>
        [JsonProperty("states", ItemConverterType = typeof(StateConverter), Required = Required.Always)]
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

            this.InitLogic?.Validate(errorMap);

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

        ILogicMetadata IStateMachineMetadata.GetScript() => this.InitLogic;

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() => this.States.Cast<IStateMetadata>();

        IOnEntryExitMetadata IStateMetadata.GetOnEntry() => throw new NotSupportedException();

        IOnEntryExitMetadata IStateMetadata.GetOnExit() => throw new NotSupportedException();

        IEnumerable<ITransitionMetadata> IStateMetadata.GetTransitions() => Enumerable.Empty<ITransitionMetadata>();
    }
}
