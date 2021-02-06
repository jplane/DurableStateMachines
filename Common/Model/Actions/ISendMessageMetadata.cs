using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DSM.Common.Model.Actions
{
    public interface ISendMessageMetadata : IActionMetadata
    {
        string Id { get; }
        TimeSpan? Delay { get; }
        string ActivityType { get; }
        (object, JObject) GetConfiguration();
    }
}
