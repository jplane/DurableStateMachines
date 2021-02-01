using System.Collections.Generic;
using System.Linq;

namespace DSM.Common.Model.States
{
    public interface IStateMetadata : IModelMetadata
    {
        string Id { get; }

        StateType Type { get; }

        bool IsDescendentOf(IStateMetadata state);

        int GetDocumentOrder();

        IOnEntryExitMetadata GetOnEntry();

        IOnEntryExitMetadata GetOnExit();

        IEnumerable<ITransitionMetadata> GetTransitions();

        IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes();

        ITransitionMetadata GetInitialTransition();

        IEnumerable<IStateMetadata> GetStates();
    }
}
