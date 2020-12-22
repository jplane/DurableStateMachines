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

        Task RemoveAsync(params string[] uniqueIds);

        Task SerializeAsync(string uniqueId, string deserializationType, Stream stream);

        Task DeserializeAsync(Func<string, string, Stream, Task> deserializeInstanceFunc);
    }
}
