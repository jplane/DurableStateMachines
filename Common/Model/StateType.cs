using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSM.Common.Model
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
