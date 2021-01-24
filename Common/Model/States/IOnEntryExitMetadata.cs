using StateChartsDotNet.Common.Model.Execution;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IOnEntryExitMetadata : IModelMetadata
    {
        bool IsEntry { get; }

        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
