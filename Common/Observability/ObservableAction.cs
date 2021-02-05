using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSM.Common.Observability
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ObservableAction
    {
        EnterStateMachine = 1,
        ExitStateMachine,
        EnterState,
        ExitState,
        MakeTransition,
        BeforeAction,
        AfterAction,
        BeforeInvokeChildStateMachine,
        AfterInvokeChildStateMachine
    }
}
