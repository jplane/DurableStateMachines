using System.Collections;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IForeachMetadata : IExecutableContentMetadata
    {
        string Item { get; }
        string Index { get; }

        IEnumerable GetArray(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
