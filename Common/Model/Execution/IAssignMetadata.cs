using System.Collections.Generic;
using System.Reflection;

namespace DSM.Common.Model.Execution
{
    public interface IAssignMetadata : IExecutableContentMetadata
    {
        (string, MemberInfo) Location { get; }
        object GetValue(dynamic data);
    }
}
