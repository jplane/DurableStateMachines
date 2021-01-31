using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IForeachMetadata : IExecutableContentMetadata
    {
        (string, MemberInfo) Item { get; }
        (string, MemberInfo) Index { get; }

        IEnumerable GetArray(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
