using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DSM.Common.Model.States;

namespace DSM.Common.Model.Actions
{
    public interface IInvokeStateMachineMetadata : IActionMetadata
    {
        string Id { get; }
        (string, MemberInfo) ResultLocation { get; }
        ChildStateMachineExecutionMode ExecutionMode { get; }
        string RemoteUri { get; }

        (string, IStateMachineMetadata) GetStateMachineInfo();
        object GetData(dynamic data);
        IEnumerable<IActionMetadata> GetCompletionActions();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChildStateMachineExecutionMode
    {
        Inline = 1,
        Remote
    }
}
