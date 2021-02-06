using System.Collections.Generic;
using System.Reflection;

namespace DSM.Common.Model.Actions
{
    public interface IAssignMetadata : IActionMetadata
    {
        (string, MemberInfo) Location { get; }
        object GetValue(dynamic data);
    }
}
