using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IStateMetadata
    {
        string Id { get; }

        bool IsDescendentOf(IStateMetadata state);

        int DepthFirstCompare(IStateMetadata metadata);

        Task<IOnEntryExitMetadata> GetOnEntry();

        Task<IOnEntryExitMetadata> GetOnExit();

        Task<IEnumerable<ITransitionMetadata>> GetTransitions();

        Task<IEnumerable<IInvokeStateChart>> GetServices();

        Task<IDatamodelMetadata> GetDatamodel();
    }
}
