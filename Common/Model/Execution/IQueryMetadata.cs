using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IQueryMetadata : IExecutableContentMetadata
    {
        string ResultLocation { get; }

        string GetType(dynamic data);
        string GetTarget(dynamic data);
        IReadOnlyDictionary<string, object> GetParams(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetExecutableContent();
    }
}
