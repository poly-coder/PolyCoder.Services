namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueContentInclusions
    {
        public bool IncludeMetadata { get; set; }
        public bool IncludeValue { get; set; }
        public bool IncludeProperties { get; set; }
    }
}