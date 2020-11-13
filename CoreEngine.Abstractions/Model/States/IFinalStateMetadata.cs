using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        Task<IDonedataMetadata> GetDonedata();
    }
}
