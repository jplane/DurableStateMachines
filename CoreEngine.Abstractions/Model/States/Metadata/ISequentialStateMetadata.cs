using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface ISequentialStateMetadata : IStateMetadata
    {
        Task<ITransitionMetadata> GetInitialTransition();
        Task<IEnumerable<IStateMetadata>> GetStates();
    }
}
