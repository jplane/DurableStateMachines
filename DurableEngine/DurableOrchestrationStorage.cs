using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class DurableOrchestrationStorage : IOrchestrationStorage
    {
        private readonly BlobContainerClient _client;
        private readonly CancellationToken _token;

        public DurableOrchestrationStorage(string connectionString, CancellationToken token)
        {
            connectionString.CheckArgNull(nameof(connectionString));

            _token = token;

            var serviceClient = new BlobServiceClient(connectionString);

            _client = serviceClient.GetBlobContainerClient("statecharts");
        }

        public Task ClearAsync()
        {
            return _client.DeleteAsync(null, _token);
        }

        public async Task DeserializeAsync(Func<string, string, Stream, Task> deserializeInstanceFunc)
        {
            deserializeInstanceFunc.CheckArgNull(nameof(deserializeInstanceFunc));

            await _client.CreateIfNotExistsAsync();

            var blobs = _client.GetBlobsAsync(traits: BlobTraits.Metadata, cancellationToken: _token);

            await foreach (var blob in blobs)
            {
                var blobClient = _client.GetBlockBlobClient(blob.Name);

                using var stream = await blobClient.OpenReadAsync(cancellationToken: _token);

                await deserializeInstanceFunc(blob.Name, blob.Metadata["deserializationtype"], stream);
            }
        }

        public async Task RemoveAsync(params string[] uniqueIds)
        {
            uniqueIds.CheckArgNull(nameof(uniqueIds));

            foreach (var uniqueId in uniqueIds)
            {
                await _client.DeleteBlobAsync(uniqueId);
            }
        }

        public async Task SerializeAsync(string uniqueId, string deserializationType, Stream stream)
        {
            uniqueId.CheckArgNull(nameof(uniqueId));
            deserializationType.CheckArgNull(nameof(deserializationType));
            stream.CheckArgNull(nameof(stream));

            var blobClient = _client.GetBlockBlobClient(uniqueId);

            using (var blobStream = await blobClient.OpenWriteAsync(true, cancellationToken: _token))
            {
                await stream.CopyToAsync(blobStream);
            }

            await blobClient.SetMetadataAsync(new Dictionary<string, string>
            {
                { "deserializationtype", deserializationType }
            });
        }
    }
}
