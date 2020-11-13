using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface IAssignMetadata : IExecutableContentMetadata
    {
        string Location { get; }
        Task<object> GetValue(dynamic data);
    }
}
