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
        private readonly Dictionary<string, (string, JObject)> _instances =
            new Dictionary<string, (string, JObject)>();

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

        public async Task DeserializeAsync(Func<string, JObject, string, Task> deserializeInstanceFunc)
        {
            deserializeInstanceFunc.CheckArgNull(nameof(deserializeInstanceFunc));

            using (await _lock.LockAsync())
            {
                foreach (var pair in _instances)
                {
                    await deserializeInstanceFunc(pair.Key, pair.Value.Item2, pair.Value.Item1);
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

        public async Task SerializeAsync(string metadataId, JObject json, string deserializationType)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            json.CheckArgNull(nameof(json));
            deserializationType.CheckArgNull(nameof(deserializationType));

            using (await _lock.LockAsync())
            {
                _instances.Add(metadataId, (deserializationType, json));
            }
        }
    }
}
