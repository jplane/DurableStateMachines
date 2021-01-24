using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IQueryMetadata : IExecutableContentMetadata
    {
        string ResultLocation { get; }
        string ActivityType { get; }
        IQueryConfiguration Configuration { get; }
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
