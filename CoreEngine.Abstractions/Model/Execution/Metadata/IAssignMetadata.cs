using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IAssignMetadata : IExecutableContentMetadata
    {
        string Location { get; }
        Task<object> GetValue(dynamic data);
    }
}
