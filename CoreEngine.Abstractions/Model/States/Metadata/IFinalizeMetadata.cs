using CoreEngine.Abstractions.Model.Execution.Metadata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IFinalizeMetadata
    {
        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();
    }
}
