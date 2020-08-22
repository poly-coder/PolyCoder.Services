using PolyCoder.Services.KeyValueStoreBase.Abstractions;
using DotNetX;
using System;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using System.Linq;
using System.IO;

namespace PolyCoder.Services.KeyValueStoreBase.Azure.Blobs
{
    public class BlobsKeyValueStoreBaseMetadataService : IKeyValueStoreBaseService
    {
        public const string PropertyKeyPrefix = "PROP-";
        public const string MetadataKeyPrefix = "META-";

        private readonly BlobContainerClient containerClient;

        public BlobsKeyValueStoreBaseMetadataService(BlobContainerClient containerClient)
        {
            this.containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        }

        public async Task<KeyValueStoreBaseCapabilitiesResponse> GetCapabilitiesAsync(
            KeyValueStoreBaseCapabilitiesRequest request, 
            CancellationToken cancellationToken)
        {
            return new KeyValueStoreBaseCapabilitiesResponse
            {
                Status = new Status { Code = 0 },
                PathSeparator = "/",
                CanList = true,
                CanFetch = true,
                CanStore = true,
                CanClear = true,
                HandlesTimeout = false,
            };
        }

        public async IAsyncEnumerable<KeyValueStoreBaseListResponse> ListAsync(
            KeyValueStoreBaseListRequest request, 
            CancellationToken cancellationToken)
        {
            bool includeValue = request.Inclusions?.IncludeValue ?? false;
            bool includeMetadata = request.Inclusions?.IncludeMetadata ?? false;
            bool includeProperties = request.Inclusions?.IncludeProperties ?? false;

            var getBlobsResult = containerClient.GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, request.KeyPrefix, cancellationToken);

            var pageSizeHint = includeValue ? 10 : 100; // Add as options

            var pageNumber = 0;

            await foreach (var page in getBlobsResult.AsPages(null, pageSizeHint))
            {
                var isLastPage = page.ContinuationToken == null;

                foreach (var window in page.Values.Windowed(pageSizeHint))
                {
                    if (includeValue)
                    {
                        var loadedItems = await Task.WhenAll(window.Select(async blobItem =>
                        {
                            var blobClient = containerClient.GetBlobClient(blobItem.Name);

                            using var memory = new MemoryStream();

                            var _downloadResponse = await blobClient.DownloadToAsync(memory, cancellationToken);

                            return (blobItem, value: memory.ToArray());
                        }));

                        var items = loadedItems.Select(pair => new KeyValueContent
                        {
                            Key = pair.blobItem.Name,
                            Metadata = includeMetadata ? ToMetadata(pair.blobItem) : null,
                            Properties = includeProperties ? ToProperties(pair.blobItem) : null,
                            Value = pair.value,
                        }).ToList();

                        yield return new KeyValueStoreBaseListResponse
                        {
                            Status = new Status { Code = 0 },
                            IsLastPage = isLastPage,
                            PageNumber = pageNumber++,
                            Items = items,
                        };
                    }
                    else
                    {
                        var items = window.Select(blobItem => new KeyValueContent
                        {
                            Key = blobItem.Name,
                            Metadata = includeMetadata ? ToMetadata(blobItem) : null,
                            Properties = includeProperties ? ToProperties(blobItem) : null,
                        }).ToList();

                        yield return new KeyValueStoreBaseListResponse
                        {
                            Status = new Status { Code = 0 },
                            IsLastPage = isLastPage,
                            PageNumber = pageNumber++,
                            Items = items,
                        };
                    }
                }
            } 
        }

        private Dictionary<string, string> ToMetadata(BlobItem blobItem)
        {
            return blobItem.Metadata
                .Where(pair => pair.Key.Length > MetadataKeyPrefix.Length && pair.Key.StartsWith(MetadataKeyPrefix))
                .ToDictionary(
                    pair => pair.Key.Substring(MetadataKeyPrefix.Length),
                    pair => pair.Value);
        }

        private KeyValueProperties ToProperties(BlobItem blobItem)
        {
            string correlationId = null, userId = null;
            blobItem.Metadata?.TryGetValue($"{PropertyKeyPrefix}CorrelationId", out correlationId);
            blobItem.Metadata?.TryGetValue($"{PropertyKeyPrefix}UserId", out userId);

            return new KeyValueProperties
            {
                CreatedAt = blobItem.Properties.CreatedOn ?? DateTimeOffset.MinValue,
                ModifiedAt = blobItem.Properties.LastModified ?? DateTimeOffset.MinValue,
                TimeToLive = blobItem.Properties.ExpiresOn,
                ContentLength = blobItem.Properties.ContentLength ?? -1,
                ContentType = blobItem.Properties.ContentType ?? "application/octet-stream",
                CorrelationId = correlationId,
                UserId = userId,
            };
        }

        public Task<KeyValueStoreBaseFetchResponse> FetchAsync(
            KeyValueStoreBaseFetchRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<KeyValueStoreBaseRemoveResponse> RemoveAsync(
            KeyValueStoreBaseRemoveRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<KeyValueStoreBaseStoreResponse> StoreAsync(
            KeyValueStoreBaseStoreRequest request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<KeyValueStoreBaseClearResponse> ClearAsync(
            KeyValueStoreBaseClearRequest request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
