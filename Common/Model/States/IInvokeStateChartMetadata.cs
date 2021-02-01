using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using DSM.Common.Model.Execution;

namespace DSM.Common.Model.States
{
    public interface IInvokeStateMachineMetadata : IModelMetadata
    {
        string Id { get; }
        (string, MemberInfo) ResultLocation { get; }
        ChildStateMachineExecutionMode ExecutionMode { get; }
        string RemoteUri { get; }

        IStateMachineMetadata GetRoot();
        string GetRootIdentifier();
        object GetData(dynamic data);
        IEnumerable<IExecutableContentMetadata> GetFinalizeExecutableContent();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChildStateMachineExecutionMode
    {
        Inline = 1,
        Remote
    }
}
