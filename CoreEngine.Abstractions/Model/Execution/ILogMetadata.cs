using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface ILogMetadata : IExecutableContentMetadata
    {
        Task<string> GetMessage(dynamic data);
    }
}
