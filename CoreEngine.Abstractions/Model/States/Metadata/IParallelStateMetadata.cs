using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IParallelStateMetadata : IStateMetadata
    {
        Task<IEnumerable<IStateMetadata>> GetStates();
    }
}
