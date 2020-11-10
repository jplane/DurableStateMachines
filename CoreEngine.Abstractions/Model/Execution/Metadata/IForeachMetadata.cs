using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IForeachMetadata : IExecutableContentMetadata
    {
        string ArrayExpression { get; }
        string Item { get; }
        string Index { get; }

        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();
    }
}
