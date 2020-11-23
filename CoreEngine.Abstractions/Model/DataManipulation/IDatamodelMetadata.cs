using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation
{
    public interface IDatamodelMetadata : IModelMetadata
    {
        IEnumerable<IDataInitMetadata> GetData();
    }
}
