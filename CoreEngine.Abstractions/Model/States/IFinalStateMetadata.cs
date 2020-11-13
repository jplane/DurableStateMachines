using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        Task<IContentMetadata> GetContent();
        Task<IEnumerable<IParamMetadata>> GetParams();
    }
}
