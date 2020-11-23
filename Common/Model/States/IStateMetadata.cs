using StateChartsDotNet.Common.Model.DataManipulation;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IStateMetadata : IModelMetadata
    {
        string Id { get; }

        bool IsDescendentOf(IStateMetadata state);

        int DepthFirstCompare(IStateMetadata metadata);

        IOnEntryExitMetadata GetOnEntry();

        IOnEntryExitMetadata GetOnExit();

        IEnumerable<ITransitionMetadata> GetTransitions();

        IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes();

        IDatamodelMetadata GetDatamodel();
    }
}
