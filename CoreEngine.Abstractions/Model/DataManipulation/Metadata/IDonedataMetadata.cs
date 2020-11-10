using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.DataManipulation.Metadata
{
    public interface IDonedataMetadata
    {
        Task<IContentMetadata> GetContent();
        Task<IEnumerable<IParamMetadata>> GetParams();
    }
}
