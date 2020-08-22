namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueStoreBaseListRequest
    {
        public string KeyPrefix { get; set; }
        public KeyValueContentInclusions Inclusions { get; set; }
    }
}