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
    /// <see cref="HistoryState{TData}"/> is a <see cref="State"/> that contains no child states. The presence of <see cref="HistoryState{TData}"/> as a child
    ///  of <see cref="CompoundState{TData}"/> or <see cref="ParallelState{TData}"/> indicates that the state machine should preserve the currently executing sub-state
    ///  when a transition out of the parent <see cref="CompoundState{TData}"/> or <see cref="ParallelState{TData}"/> occurs. If later a transition occurs back to
    ///  the <see cref="HistoryState{TData}"/>, the saved "last executing state" is then re-entered. For more information see: https://statecharts.github.io/glossary/history-state.html
    /// </summary>
    /// <typeparam name="TData">The execution state of the state machine.</typeparam>
    public class HistoryState<TData> : State<TData>, IHistoryStateMetadata
    {
        public HistoryState()
        {
        }

        /// <summary>
        /// There are two types of <see cref="HistoryState{TData}"/> behavior: shallow and deep.
        ///  Shallow history tracks only the immediate sub-state. Deep history tracks (and will re-enter) all current nested sub-states.
        /// </summary>
        [JsonProperty("deep")]
        public bool IsDeep { get; set; }

        internal override void Validate(IDictionary<string, List<string>> errorMap)
        {
            Debug.Assert(errorMap != null);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Id))
            {
                errors.Add("Id is invalid.");
            }

            if (errors.Any())
            {
                errorMap.Add(((IModelMetadata)this).MetadataId, errors);
            }
        }

        StateType IStateMetadata.Type => StateType.History;

        int IStateMetadata.GetDocumentOrder()
        {
            return this.GetDocumentOrder();
        }

        bool IStateMetadata.IsDescendentOf(IStateMetadata state)
        {
            return this.IsDescendentOf(state);
        }

        ITransitionMetadata IStateMetadata.GetInitialTransition() => throw new NotSupportedException();

        IOnEntryExitMetadata IStateMetadata.GetOnEntry() => throw new NotSupportedException();

        IOnEntryExitMetadata IStateMetadata.GetOnExit() => throw new NotSupportedException();

        IEnumerable<IInvokeStateMachineMetadata> IStateMetadata.GetStateMachineInvokes() => throw new NotSupportedException();

        IEnumerable<IStateMetadata> IStateMetadata.GetStates() => throw new NotSupportedException();

        IEnumerable<ITransitionMetadata> IStateMetadata.GetTransitions() => throw new NotSupportedException();
    }
}
