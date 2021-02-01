using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSM.Common.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransitionType
    {
        Internal,
        External
    }
}
