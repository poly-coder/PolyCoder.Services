namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueStoreBaseStoreResponse
    {
        public Status Status { get; set; }
        public KeyValueProperties Properties { get; set; }
    }
}