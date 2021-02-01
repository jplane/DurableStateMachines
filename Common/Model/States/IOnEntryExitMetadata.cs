using DSM.Common.Model.Execution;
using System.Collections.Generic;

namespace DSM.Common.Model.States
{
    public interface IOnEntryExitMetadata : IModelMetadata
    {
        bool IsEntry { get; }

        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
