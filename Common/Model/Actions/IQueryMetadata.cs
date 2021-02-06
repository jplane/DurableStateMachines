using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DSM.Common.Model.Actions
{
    public interface IQueryMetadata : IActionMetadata
    {
        (string, MemberInfo) ResultLocation { get; }
        string ActivityType { get; }
        IEnumerable<IActionMetadata> GetActions();
        (object, JObject) GetConfiguration();
    }
}
