namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueStoreBaseCapabilitiesResponse
    {
        public Status Status { get; set; }
        public bool CanList { get; set; }
        public bool CanFetch { get; set; }
        public bool CanStore { get; set; }
        public bool CanClear { get; set; }
        public bool HandlesTimeout { get; set; }
        public string PathSeparator { get; set; }
    }
}