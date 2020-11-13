using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IRootStateMetadata : IStateMetadata
    {
        Databinding Databinding { get; }

        Task<IEnumerable<IStateMetadata>> GetStates();
        Task<ITransitionMetadata> GetInitialTransition();
        Task<IScriptMetadata> GetScript();
    }
}
