using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        Task<IDonedataMetadata> GetDonedata();
    }
}
