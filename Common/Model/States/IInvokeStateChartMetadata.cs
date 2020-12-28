using System;
using System.Collections.Generic;
using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IInvokeStateChartMetadata : IModelMetadata
    {
        string Id { get; }
        string IdLocation { get; }
        ChildStateChartExecutionMode ExecutionMode { get; }
        string RemoteUri { get; }

        IStateChartMetadata GetRoot();
        IReadOnlyDictionary<string, object> GetParams(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetFinalizeExecutableContent();
    }

    public enum ChildStateChartExecutionMode
    {
        Inline = 1,
        Isolated,
        Remote
    }
}
