﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSM.Common;
using DSM.Common.Model;
using DSM.Common.Model.Actions;
using DSM.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DSM.Metadata.States;

namespace DSM.Metadata.Actions
{
    /// <summary>
    /// <see cref="InvokeStateMachine{TData}"/> models the execution of a <see cref="StateMachine{TData}"/> as a child of the currently executing <see cref="StateMachine{TData}"/>.
    /// It's permitted that child state machines may themselves have children, as well.
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    [JsonObject(Id = "InvokeStateMachine",
                ItemNullValueHandling = NullValueHandling.Ignore,
                ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
    public sealed class InvokeStateMachine<TData> : Action<TData>, IInvokeStateMachineMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _getData;

        private MemberInfo _resultTarget;
        private MetadataList<Actions.Action<TData>> _actions;

        public InvokeStateMachine()
        {
            this.ExecutionMode = ChildStateMachineExecutionMode.Inline;
            this.CompletionActions = new MetadataList<Actions.Action<TData>>();

            _getData = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.InputExpression))
                {
                    return ExpressionCompiler.Compile<object>(((IModelMetadata)this).MetadataId, this.InputExpression);
                }
                else if (this.InputFunction != null)
                {
                    return data => this.InputFunction((TData) data);
                }
                else
                {
                    return _ => this.Input;
                }
            });
        }

        string IModelMetadata.MetadataId => this.MetadataIdResolver?.Invoke(this);

        /// <summary>
        /// Unique identifier for this <see cref="InvokeStateMachine{TData}"/>.
        /// Used to disambiguate 'done.invoke.[Id]' events received by the parent upon completion of child execution.
        /// </summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Target field or property in <typeparamref name="TData"/> into which the invocation result should be assigned.
        /// </summary>
        [JsonIgnore]
        public Expression<Func<TData, object>> AssignTo
        {
            set => _resultTarget = value.ExtractMember(nameof(AssignTo));
        }

        [JsonProperty("resultlocation", Required = Required.DisallowNull)]
        private string ResultLocation { get; set; }

        /// <summary>
        /// <see cref="ChildStateMachineExecutionMode.Inline"/> executes the child state machine as a Durable Functions sub-orchestration.
        /// <see cref="ChildStateMachineExecutionMode.Remote"/> executes the child state machine as an HTTP-202-style remote invocation. See <see cref="RemoteUri"/>.
        /// </summary>
        [JsonProperty("mode", Required = Required.DisallowNull)]
        public ChildStateMachineExecutionMode ExecutionMode { get; set; }

        /// <summary>
        /// Remote endpoint for a child state machine configured with <see cref="ExecutionMode"/> = <see cref="ChildStateMachineExecutionMode.Remote"/>.
        /// </summary>
        [JsonProperty("remoteuri", Required = Required.DisallowNull)]
        public string RemoteUri { get; set; }

        /// <summary>
        /// Function to dynamically generate the child state machine input, using execution state <typeparamref name="TData"/> from the parent state machine.
        /// To use a static value, use <see cref="Input"/>.
        /// </summary>
        [JsonIgnore]
        public Func<TData, object> InputFunction { get; set; }

        /// <summary>
        /// Static value for child state machine input.
        /// To use a dynamically generated value using the execution state <see cref="TData"/>, use <see cref="InputFunction"/>.
        /// </summary>
        [JsonProperty("input", Required = Required.DisallowNull)]
        public object Input { get; set; }

        [JsonProperty("inputexpression", Required = Required.DisallowNull)]
        private string InputExpression { get; set; }

        /// <summary>
        /// Unique identifier for the child state machine definition. This must resolve to a valid definition on the local or remote Durable Functions instance.
        /// </summary>
        [JsonProperty("statemachineidentifier", Required = Required.DisallowNull)]
        public string StateMachineIdentifier { get; set; }

        [JsonProperty("statemachinedefinition", Required = Required.DisallowNull)]
        private JObject Definition { get; set; }

        /// <summary>
        /// The set of actions executed for this <see cref="InvokeStateMachine{TData}"/> once the child invocation successfully completes.
        /// </summary>
        [JsonProperty("completionactions", ItemConverterType = typeof(ActionConverter), Required = Required.DisallowNull)]
        public MetadataList<Actions.Action<TData>> CompletionActions
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

                value.MetadataIdResolver = () => $"{this.MetadataIdResolver?.Invoke(this) ?? "invoke"}.actions";

                _actions = value;
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

            if (this.ExecutionMode == ChildStateMachineExecutionMode.Remote &&
                string.IsNullOrWhiteSpace(this.RemoteUri))
            {
                errors.Add("ChildStateMachineExecutionMode.Remote requires a RemoteUri value.");
            }

            if (string.IsNullOrWhiteSpace(this.StateMachineIdentifier) && this.Definition == null)
            {
                errors.Add("Invoke operation must specify either a state machine identifier or definition.");
            }

            foreach (var action in this.CompletionActions)
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

                info["metadataId"] = ((IModelMetadata) this).MetadataId;

                return info;
            }
        }

        IEnumerable<IActionMetadata> IInvokeStateMachineMetadata.GetCompletionActions() =>
            this.CompletionActions ?? Enumerable.Empty<IActionMetadata>();

        (string, IStateMachineMetadata) IInvokeStateMachineMetadata.GetStateMachineInfo() =>
            (this.StateMachineIdentifier, this.Definition?.ToObject<StateMachine<Dictionary<string, object>>>());

        object IInvokeStateMachineMetadata.GetData(dynamic data) => _getData.Value(data);

        (string, MemberInfo) IInvokeStateMachineMetadata.ResultLocation => (this.ResultLocation, _resultTarget);
    }
}
