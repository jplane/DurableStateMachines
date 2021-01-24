using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StateChartsDotNet.Common.Debugger
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DebuggerAction
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
