using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IRaiseMetadata : IExecutableContentMetadata
    {
        string GetMessage(dynamic data);
    }
}
