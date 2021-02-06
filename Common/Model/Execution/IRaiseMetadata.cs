using System.Collections.Generic;

namespace DSM.Common.Model.Execution
{
    public interface IRaiseMetadata : IActionMetadata
    {
        string GetMessage(dynamic data);
    }
}
