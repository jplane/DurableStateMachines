using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using StateChartsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class DurableOrchestrationStorage : IOrchestrationStorage
    {
        private readonly BlobContainerClient _client;
        private readonly CancellationToken _token;

        public DurableOrchestrationStorage(string connectionString, string hubName, CancellationToken token)
        {
            connectionString.CheckArgNull(nameof(connectionString));

            _token = token;

            var serviceClient = new BlobServiceClient(connectionString);

            _client = serviceClient.GetBlobContainerClient($"{hubName}-statecharts");
        }

        public Task ClearAsync()
        {
            return _client.DeleteAsync(null, _token);
        }

        public async Task DeserializeAsync(string metadataId, Func<string, Stream, string, Task> deserializeInstanceFunc)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            deserializeInstanceFunc.CheckArgNull(nameof(deserializeInstanceFunc));

            await _client.CreateIfNotExistsAsync();

            var blobClient = _client.GetBlobClient(metadataId);

            if (! await blobClient.ExistsAsync())
            {
                return;
            }

            var metadataResponse = await blobClient.GetPropertiesAsync();

            Debug.Assert(metadataResponse != null);

            using var stream = await blobClient.OpenReadAsync(cancellationToken: _token);

            await deserializeInstanceFunc(metadataId, stream, metadataResponse.Value.Metadata["metadatatype"]);
        }

        public async Task DeserializeAllAsync(Func<string, Stream, string, Task> deserializeInstanceFunc)
        {
            deserializeInstanceFunc.CheckArgNull(nameof(deserializeInstanceFunc));

            await _client.CreateIfNotExistsAsync();

            var blobs = _client.GetBlobsAsync(traits: BlobTraits.Metadata, cancellationToken: _token);

            await foreach (var blob in blobs)
            {
                var blobClient = _client.GetBlockBlobClient(blob.Name);

                using var stream = await blobClient.OpenReadAsync(cancellationToken: _token);

                await deserializeInstanceFunc(blob.Name, stream, blob.Metadata["metadatatype"]);
            }
        }

        public async Task RemoveAsync(params string[] metadataIds)
        {
            metadataIds.CheckArgNull(nameof(metadataIds));

            foreach (var metadataId in metadataIds)
            {
                await _client.DeleteBlobAsync(metadataId);
            }
        }

        public async Task SerializeAsync(string metadataId, Stream stream, string metadataType)
        {
            metadataId.CheckArgNull(nameof(metadataId));
            stream.CheckArgNull(nameof(stream));
            metadataType.CheckArgNull(nameof(metadataType));

            var blobClient = _client.GetBlockBlobClient(metadataId);

            using (var blobStream = await blobClient.OpenWriteAsync(true, cancellationToken: _token))
            {
                await stream.CopyToAsync(blobStream, 81920 /* documented default */, _token);
            }

            await blobClient.SetMetadataAsync(new Dictionary<string, string>
            {
                { "metadatatype", metadataType }
            });
        }
    }
}
