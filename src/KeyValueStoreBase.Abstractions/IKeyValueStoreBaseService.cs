namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public interface IKeyValueStoreBaseService :
        IKeyValueStoreBaseMetadataService,
        IKeyValueStoreBaseListerService,
        IKeyValueStoreBaseFetcherService,
        IKeyValueStoreBaseStorerService,
        IKeyValueStoreBaseClearerService
    {
    }
}