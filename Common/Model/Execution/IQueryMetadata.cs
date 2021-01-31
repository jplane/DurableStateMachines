using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IQueryMetadata : IExecutableContentMetadata
    {
        (string, MemberInfo) ResultLocation { get; }
        string ActivityType { get; }
        IQueryConfiguration Configuration { get; }
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
