using CoreEngine.Abstractions.Model.Execution.Metadata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IRootStateMetadata : IStateMetadata
    {
        Databinding Databinding { get; }

        Task<IEnumerable<IStateMetadata>> GetStates();
        Task<ITransitionMetadata> GetInitialTransition();
        Task<IScriptMetadata> GetScript();
    }
}
