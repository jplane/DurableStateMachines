using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    public interface IOrchestrationStorage
    {
        Task ClearAsync();

        Task RemoveAsync(params string[] metadataIds);

        Task SerializeAsync(string metadataId, JObject json, string deserializationType);

        Task DeserializeAsync(Func<string, JObject, string, Task> deserializeInstanceFunc);
    }
}
