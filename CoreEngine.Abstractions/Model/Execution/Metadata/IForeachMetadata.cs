using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IForeachMetadata : IExecutableContentMetadata
    {
        string Item { get; }
        string Index { get; }

        Task<IEnumerable> GetArray(dynamic data);
        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();
    }
}
