namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueStoreBaseFetchRequest
    {
        public string Key { get; set; }
        public KeyValueContentInclusions Inclusions { get; set; }
    }
}