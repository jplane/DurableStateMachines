using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using DSM.Common.Model.Execution;

namespace DSM.Common.Model.States
{
    public interface IInvokeStateChartMetadata : IModelMetadata
    {
        string Id { get; }
        (string, MemberInfo) ResultLocation { get; }
        ChildStateChartExecutionMode ExecutionMode { get; }
        string RemoteUri { get; }

        IStateChartMetadata GetRoot();
        string GetRootIdentifier();
        object GetData(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetFinalizeExecutableContent();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChildStateChartExecutionMode
    {
        Inline = 1,
        Remote
    }
}
