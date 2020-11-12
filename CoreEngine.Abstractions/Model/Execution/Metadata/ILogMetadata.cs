using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface ILogMetadata : IExecutableContentMetadata
    {
        Task<string> GetMessage(dynamic data);
    }
}
