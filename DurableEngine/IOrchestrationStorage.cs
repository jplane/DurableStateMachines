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

        Task SerializeAsync(string metadataId, Stream stream, string deserializationType);

        Task DeserializeAsync(string metadataId, Func<string, Stream, string, Task> deserializeInstanceFunc);

        Task DeserializeAllAsync(Func<string, Stream, string, Task> deserializeInstanceFunc);
    }
}
