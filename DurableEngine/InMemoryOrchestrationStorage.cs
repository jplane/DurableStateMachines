using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    public class InMemoryOrchestrationStorage : IOrchestrationStorage
    {
        private readonly AsyncLock _lock;
        private readonly Dictionary<string, (string, byte[])> _instances =
            new Dictionary<string, (string, byte[])>();

        public InMemoryOrchestrationStorage()
        {
            _lock = new AsyncLock();
        }

        public async Task ClearAsync()
        {
            using (await _lock.LockAsync())
            {
                _instances.Clear();
            }
        }

        public async Task DeserializeAsync(Func<string, Stream, string, Task> deserializeInstanceFunc)
        {
            deserializeInstanceFunc.CheckArgNull(nameof(deserializeInstanceFunc));

            using (await _lock.LockAsync())
            {
                foreach (var pair in _instances)
                {
                    using var stream = new MemoryStream(pair.Value.Item2);

                    await deserializeInstanceFunc(pair.Key, stream, pair.Value.Item1);
                }
            } 
        }

        public async Task RemoveAsync(params string[] metadataIds)
        {
            metadataIds.CheckArgNull(nameof(metadataIds));

            using (await _lock.LockAsync())
            {
                foreach (var metadataId in metadataIds)
                {
                    _instances.Remove(metadataId);
                }
            } 
        }

        public async Task SerializeAsync(string metadataId, Stream stream, string deserializationType)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            stream.CheckArgNull(nameof(stream));
            deserializationType.CheckArgNull(nameof(deserializationType));

            using (await _lock.LockAsync())
            {
                using var copy = new MemoryStream();

                await stream.CopyToAsync(copy);

                copy.Position = 0;

                _instances.Add(metadataId, (deserializationType, copy.ToArray()));
            }
        }
    }
}
