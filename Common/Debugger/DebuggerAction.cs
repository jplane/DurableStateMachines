using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.Common.Debugger
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DebuggerAction
    {
        EnterStateMachine = 1,
        ExitStateMachine,
        EnterState,
        ExitState
    }
}
