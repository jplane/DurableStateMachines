using System.Collections.Generic;

namespace DSM.Common.Model.Execution
{
    public interface IRaiseMetadata : IExecutableContentMetadata
    {
        string GetMessage(dynamic data);
    }
}
