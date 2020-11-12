using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IScriptMetadata : IExecutableContentMetadata
    {
        Task Execute(dynamic data);
    }
}
