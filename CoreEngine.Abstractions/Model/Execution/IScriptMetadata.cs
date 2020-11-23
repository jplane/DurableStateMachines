using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface IScriptMetadata : IExecutableContentMetadata
    {
        void Execute(dynamic data);
    }
}
