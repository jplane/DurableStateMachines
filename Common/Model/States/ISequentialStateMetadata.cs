using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface ISequentialStateMetadata : IStateMetadata
    {
        ITransitionMetadata GetInitialTransition();
        IEnumerable<IStateMetadata> GetStates();
    }
}
