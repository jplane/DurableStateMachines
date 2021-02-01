using Newtonsoft.Json;
using DSM.Common.Model;
using DSM.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DSM.Metadata.States
{
    public class HistoryState<TData> : State<TData>, IHistoryStateMetadata
    {
        public HistoryState()
        {
        }

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
