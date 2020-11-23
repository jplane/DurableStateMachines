using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IOnEntryExitMetadata : IModelMetadata
    {
        bool IsEntry { get; }

        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
