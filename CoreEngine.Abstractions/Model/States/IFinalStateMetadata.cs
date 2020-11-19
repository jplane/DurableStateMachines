using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        IContentMetadata GetContent();
        IEnumerable<IParamMetadata> GetParams();
    }
}
