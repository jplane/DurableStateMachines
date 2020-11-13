using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface ISequentialStateMetadata : IStateMetadata
    {
        Task<ITransitionMetadata> GetInitialTransition();
        Task<IEnumerable<IStateMetadata>> GetStates();
    }
}
