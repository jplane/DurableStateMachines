using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IParallelStateMetadata : IStateMetadata
    {
        Task<IEnumerable<IStateMetadata>> GetStates();
    }
}
