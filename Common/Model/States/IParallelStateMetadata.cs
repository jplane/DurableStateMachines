using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IParallelStateMetadata : IStateMetadata
    {
        IEnumerable<IStateMetadata> GetStates();
    }
}
