using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.DataManipulation.Metadata
{
    public interface IDatamodelMetadata
    {
        Task<IEnumerable<IDataMetadata>> GetData();
    }
}
