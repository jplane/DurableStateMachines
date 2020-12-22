﻿using Nito.AsyncEx;
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

        public async Task DeserializeAsync(Func<string, string, Stream, Task> deserializeInstanceFunc)
        {
            deserializeInstanceFunc.CheckArgNull(nameof(deserializeInstanceFunc));

            using (await _lock.LockAsync())
            {
                foreach (var pair in _instances)
                {
                    using var stream = new MemoryStream(pair.Value.Item2);

                    await deserializeInstanceFunc(pair.Key, pair.Value.Item1, stream);
                }
            } 
        }

        public async Task RemoveAsync(params string[] uniqueIds)
        {
            uniqueIds.CheckArgNull(nameof(uniqueIds));

            using (await _lock.LockAsync())
            {
                foreach (var uniqueId in uniqueIds)
                {
                    _instances.Remove(uniqueId);
                }
            } 
        }

        public async Task SerializeAsync(string uniqueId, string deserializationType, Stream stream)
        {
            uniqueId.CheckArgNull(nameof(uniqueId));
            deserializationType.CheckArgNull(nameof(deserializationType));
            stream.CheckArgNull(nameof(stream));

            using (await _lock.LockAsync())
            {
                using var ms = new MemoryStream();

                await stream.CopyToAsync(ms);

                _instances.Add(uniqueId, (deserializationType, ms.ToArray()));
            }
        }
    }
}
