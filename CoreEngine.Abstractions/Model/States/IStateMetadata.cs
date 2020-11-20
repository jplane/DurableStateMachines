using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IStateMetadata : IModelMetadata
    {
        string Id { get; }

        bool IsDescendentOf(IStateMetadata state);

        int DepthFirstCompare(IStateMetadata metadata);

        IOnEntryExitMetadata GetOnEntry();

        IOnEntryExitMetadata GetOnExit();

        IEnumerable<ITransitionMetadata> GetTransitions();

        IEnumerable<IInvokeStateChart> GetServices();

        IDatamodelMetadata GetDatamodel();
    }
}
