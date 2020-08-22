using System.Threading;
using System.Threading.Tasks;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public interface IKeyValueStoreBaseMetadataService
    {
        Task<KeyValueStoreBaseCapabilitiesResponse> GetCapabilitiesAsync(KeyValueStoreBaseCapabilitiesRequest request, CancellationToken cancellationToken);
    }
}