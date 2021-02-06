using System.Collections.Generic;

namespace DSM.Common.Model.Actions
{
    public interface IRaiseMetadata : IActionMetadata
    {
        string GetMessage(dynamic data);
    }
}
