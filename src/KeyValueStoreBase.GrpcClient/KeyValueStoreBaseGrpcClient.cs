using DotNetX;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PolyCoder.Services.KeyValueStoreBase.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static PolyCoder.Services.KeyValueStoreBase.Protocol.KeyValueStoreBaseClearerService;
using static PolyCoder.Services.KeyValueStoreBase.Protocol.KeyValueStoreBaseFetcherService;
using static PolyCoder.Services.KeyValueStoreBase.Protocol.KeyValueStoreBaseListerService;
using static PolyCoder.Services.KeyValueStoreBase.Protocol.KeyValueStoreBaseMetadataService;
using static PolyCoder.Services.KeyValueStoreBase.Protocol.KeyValueStoreBaseStorerService;

namespace PolyCoder.Services.KeyValueStoreBase.GrpcClient
{
    public class KeyValueStoreBaseGrpcClient : IKeyValueStoreBaseService
    {
        private readonly KeyValueStoreBaseMetadataServiceClient metadataClient;
        private readonly KeyValueStoreBaseListerServiceClient listerClient;
        private readonly KeyValueStoreBaseFetcherServiceClient fetcherClient;
        private readonly KeyValueStoreBaseStorerServiceClient storerClient;
        private readonly KeyValueStoreBaseClearerServiceClient clearerClient;
        private readonly IClock clock;
        private readonly CallCredentials callCredentials;
        private readonly TimeSpan? deadlineOnMessages;
        private readonly WriteOptions writeOptions;

        public KeyValueStoreBaseGrpcClient(
            KeyValueStoreBaseMetadataServiceClient metadataClient,
            KeyValueStoreBaseListerServiceClient listerClient,
            KeyValueStoreBaseFetcherServiceClient fetcherClient,
            KeyValueStoreBaseStorerServiceClient storerClient,
            KeyValueStoreBaseClearerServiceClient clearerClient,
            IClock clock,
            CallCredentials callCredentials,
            TimeSpan? deadlineOnMessages,
            WriteOptions writeOptions)
        {
            if (deadlineOnMessages.HasValue && deadlineOnMessages.Value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(deadlineOnMessages), $"Must be a positive TimeSpan");
            }

            this.metadataClient = metadataClient ?? throw new ArgumentNullException(nameof(metadataClient));
            this.listerClient = listerClient ?? throw new ArgumentNullException(nameof(listerClient));
            this.fetcherClient = fetcherClient ?? throw new ArgumentNullException(nameof(fetcherClient));
            this.storerClient = storerClient ?? throw new ArgumentNullException(nameof(storerClient));
            this.clearerClient = clearerClient ?? throw new ArgumentNullException(nameof(clearerClient));
            this.clock = clock;
            this.callCredentials = callCredentials;
            this.deadlineOnMessages = deadlineOnMessages;
            this.writeOptions = writeOptions;
        }

        public async Task<KeyValueStoreBaseCapabilitiesResponse> GetCapabilitiesAsync(
            KeyValueStoreBaseCapabilitiesRequest request,
            CancellationToken cancellationToken)
        {
            var input = new Protocol.KeyValueStoreBaseCapabilitiesRequest();

            var callOptions = PrepareCallOptions(cancellationToken);

            var output = await metadataClient.CapabilitiesAsync(input, callOptions);

            var response = new KeyValueStoreBaseCapabilitiesResponse
            {
                Status = ToStatus(output.Status),
                HandlesTimeout = output.HandlesTimeout,
                PathSeparator = output.PathSeparator,
                CanList = output.CanList,
                CanFetch = output.CanFetch,
                CanStore = output.CanStore,
                CanClear = output.CanClear,
            };

            return response;
        }

        public async IAsyncEnumerable<KeyValueStoreBaseListResponse> ListAsync(
            KeyValueStoreBaseListRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var input = new Protocol.KeyValueStoreBaseListRequest
            {
                KeyPrefix = request.KeyPrefix,
                Inclusions = OfInclusions(request.Inclusions),
            };

            var callOptions = PrepareCallOptions(cancellationToken);

            var outputStream = listerClient.List(input, callOptions);

            while (await outputStream.ResponseStream.MoveNext(cancellationToken))
            {
                var output = outputStream.ResponseStream.Current;

                var response = new KeyValueStoreBaseListResponse
                {
                    Status = ToStatus(output.Status),
                    IsLastPage = output.IsLastPage,
                    PageNumber = output.PageNumber,
                    Items = output.Items
                        .Select(item =>
                        {
                            return new KeyValueContent
                            {
                                Key = item.Key,
                                Value = ToBytes(item.Value),
                                Metadata = ToMetadata(item.Metadata),
                                Properties = ToProperties(item.Properties),
                            };
                        })
                        .ToList()
                };

                yield return response;
            }
        }

        private byte[] ToBytes(ByteString value)
        {
            return value == null || value.IsEmpty ? null : value.ToByteArray();
        }

        public async Task<KeyValueStoreBaseFetchResponse> FetchAsync(
            KeyValueStoreBaseFetchRequest request,
            CancellationToken cancellationToken)
        {
            var input = new Protocol.KeyValueStoreBaseFetchRequest
            {
                Key = request.Key,
                Inclusions = OfInclusions(request.Inclusions),
            };

            var callOptions = PrepareCallOptions(cancellationToken);

            var output = await fetcherClient.FetchAsync(input, callOptions);

            var response = new KeyValueStoreBaseFetchResponse
            {
                Status = ToStatus(output.Status),
                Found = output.Found,
                Content = ToKeyValueContent(output.Content),
            };

            return response;
        }

        public async Task<KeyValueStoreBaseRemoveResponse> RemoveAsync(
            KeyValueStoreBaseRemoveRequest request,
            CancellationToken cancellationToken)
        {
            var input = new Protocol.KeyValueStoreBaseRemoveRequest
            {
                Key = request.Key,
            };

            var callOptions = PrepareCallOptions(cancellationToken);

            var output = await storerClient.RemoveAsync(input, callOptions);

            var response = new KeyValueStoreBaseRemoveResponse
            {
                Status = ToStatus(output.Status),
            };

            return response;
        }

        public async Task<KeyValueStoreBaseStoreResponse> StoreAsync(
            KeyValueStoreBaseStoreRequest request,
            CancellationToken cancellationToken)
        {
            var input = new Protocol.KeyValueStoreBaseStoreRequest
            {
                Key = request.Key,
                Value = OfBytes(request.Value),
                ContentType = request.ContentType,
                CorrelationId = request.CorrelationId,
                UserId = request.UserId,
                TimeoutKind = OfTimeoutKind(request.TimeoutKind),
                TimeoutDate = OfDateTimeOffset(request.TimeoutDate),
                TimeoutSpan = OfTimeSpanMinutes(request.TimeoutSpan),
            };

            input.Metadata.Add(request.Metadata);

            var callOptions = PrepareCallOptions(cancellationToken);

            var output = await storerClient.StoreAsync(input, callOptions);

            var response = new KeyValueStoreBaseStoreResponse
            {
                Status = ToStatus(output.Status),
                Properties = ToProperties(output.Properties),
            };

            return response;
        }

        public async Task<KeyValueStoreBaseClearResponse> ClearAsync(
            KeyValueStoreBaseClearRequest request,
            CancellationToken cancellationToken)
        {
            var input = new Protocol.KeyValueStoreBaseClearRequest
            {
                KeyPrefix = request.KeyPrefix,
            };

            var callOptions = PrepareCallOptions(cancellationToken);

            var output = await clearerClient.ClearAsync(input, callOptions);

            var response = new KeyValueStoreBaseClearResponse
            {
                Status = ToStatus(output.Status),
            };

            return response;
        }

        private CallOptions PrepareCallOptions(CancellationToken cancellationToken)
        {
            var callOptions = new CallOptions().WithCancellationToken(cancellationToken);

            if (callCredentials != null)
            {
                callOptions.WithCredentials(callCredentials);
            }

            if (deadlineOnMessages.HasValue)
            {
                var date = clock.GetUtcNow();
                date = date + deadlineOnMessages.Value;
                callOptions.WithDeadline(date);
            }

            if (writeOptions != null)
            {
                callOptions.WithWriteOptions(writeOptions);
            }

            return callOptions;
        }

        private Abstractions.Status ToStatus(Google.Rpc.Status status)
        {
            if (status == null)
            {
                return null;
            }

            return new Abstractions.Status
            {
                Code = status.Code,
                Message = status.Message,
            };
        }

        private KeyValueContent ToKeyValueContent(Protocol.KeyValueContent content)
        {
            if (content == null)
            {
                return null;
            }

            return new KeyValueContent
            {
                Key = content.Key,
                Value = content.Value == null || content.Value.IsEmpty ? null : content.Value.ToByteArray(),
                Metadata = ToMetadata(content.Metadata),
                Properties = ToProperties(content.Properties),
            };
        }

        private KeyValueProperties ToProperties(Protocol.KeyValueProperties properties)
        {
            if (properties == null)
            {
                return null;
            }

            return new KeyValueProperties
            {
                ContentType = properties.ContentType,
                ContentLength = properties.ContentLength,
                CreatedAt = properties.CreatedAt.ToDateTimeOffset(),
                ModifiedAt = properties.ModifiedAt.ToDateTimeOffset(),
                TimeToLive = properties.TimeToLive.ToDateTimeOffset(),
                CorrelationId = properties.CorrelationId,
                UserId = properties.UserId,
            };
        }

        private Dictionary<string, string> ToMetadata(MapField<string, string> metadata)
        {
            if (metadata == null)
            {
                return null;
            }

            return metadata.ToDictionary();
        }

        private Protocol.KeyValueContentInclusions OfInclusions(KeyValueContentInclusions inclusions)
        {
            if (inclusions == null)
            {
                return null;
            }

            return new Protocol.KeyValueContentInclusions
            {
                IncludeMetadata = inclusions.IncludeMetadata,
                IncludeProperties = inclusions.IncludeProperties,
                IncludeValue = inclusions.IncludeValue,
            };
        }

        private Protocol.TimeoutKind OfTimeoutKind(TimeoutKind timeoutKind)
        {
            return timeoutKind switch
            {
                TimeoutKind.None => Protocol.TimeoutKind.None,
                TimeoutKind.Default => Protocol.TimeoutKind.Default,
                TimeoutKind.Date => Protocol.TimeoutKind.Date,
                TimeoutKind.TimespanMinutes => Protocol.TimeoutKind.TimespanMinutes,
                TimeoutKind.Permanent => Protocol.TimeoutKind.Permanent,
                _ => (Protocol.TimeoutKind)(-1),
            };
        }

        private static ByteString OfBytes(byte[] bytes)
        {
            return bytes == null ? ByteString.Empty : ByteString.CopyFrom(bytes);
        }

        private static Timestamp OfDateTimeOffset(DateTimeOffset? date)
        {
            return date.HasValue ? Timestamp.FromDateTimeOffset(date.Value) : null;
        }

        private static int OfTimeSpanMinutes(TimeSpan? timespan)
        {
            return timespan.HasValue ? (int)timespan.Value.TotalMinutes : 0;
        }
    }
}
