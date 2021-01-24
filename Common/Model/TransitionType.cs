using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StateChartsDotNet.Common.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransitionType
    {
        Internal,
        External
    }
}
