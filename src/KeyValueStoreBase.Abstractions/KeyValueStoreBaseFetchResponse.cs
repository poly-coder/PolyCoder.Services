namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueStoreBaseFetchResponse
    {
        public Status Status { get; set; }
        public bool Found { get; set; }
        public KeyValueContent Content { get; set; }
    }
}