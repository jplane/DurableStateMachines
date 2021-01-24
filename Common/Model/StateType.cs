using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StateChartsDotNet.Common.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StateType
    {
        Atomic = 1,
        Compound,
        Parallel,
        History,
        Final,
        Root
    }
}
