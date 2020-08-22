using System.Threading;
using System.Threading.Tasks;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public interface IKeyValueStoreBaseClearerService
    {
        Task<KeyValueStoreBaseClearResponse> ClearAsync(KeyValueStoreBaseClearRequest request, CancellationToken cancellationToken);
    }
}