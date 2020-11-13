using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface IScriptMetadata : IExecutableContentMetadata
    {
        Task Execute(dynamic data);
    }
}
