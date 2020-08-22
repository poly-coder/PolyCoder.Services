using System.Threading;
using System.Threading.Tasks;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public interface IKeyValueStoreBaseFetcherService
    {
        Task<KeyValueStoreBaseFetchResponse> FetchAsync(KeyValueStoreBaseFetchRequest request, CancellationToken cancellationToken);
    }
}