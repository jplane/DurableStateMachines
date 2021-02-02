using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DSM.Common.Model.Execution
{
    public interface IQueryMetadata : IExecutableContentMetadata
    {
        (string, MemberInfo) ResultLocation { get; }
        string ActivityType { get; }
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
        (IQueryConfiguration, JObject) GetConfiguration();
    }
}
