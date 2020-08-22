using System.Threading;
using System.Threading.Tasks;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public interface IKeyValueStoreBaseStorerService
    {
        Task<KeyValueStoreBaseStoreResponse> StoreAsync(KeyValueStoreBaseStoreRequest request, CancellationToken cancellationToken);
        Task<KeyValueStoreBaseRemoveResponse> RemoveAsync(KeyValueStoreBaseRemoveRequest request, CancellationToken cancellationToken);
    }
}