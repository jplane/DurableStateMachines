using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface ISequentialStateMetadata : IStateMetadata
    {
        ITransitionMetadata GetInitialTransition();
        IEnumerable<IStateMetadata> GetStates();
    }
}
