using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface IForeachMetadata : IExecutableContentMetadata
    {
        string Item { get; }
        string Index { get; }

        IEnumerable GetArray(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
