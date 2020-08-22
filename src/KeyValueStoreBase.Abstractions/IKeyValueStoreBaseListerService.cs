using System.Collections.Generic;
using System.Threading;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public interface IKeyValueStoreBaseListerService
    {
        IAsyncEnumerable<KeyValueStoreBaseListResponse> ListAsync(KeyValueStoreBaseListRequest request, CancellationToken cancellationToken);
    }
}