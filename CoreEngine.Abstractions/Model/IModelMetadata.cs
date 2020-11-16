using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model
{
    public interface IModelMetadata
    {
        IRootStateMetadata GetRootState();
    }
}
