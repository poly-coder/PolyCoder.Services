using System.Collections.Generic;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueContent
    {
        public string Key { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public byte[] Value { get; set; }
        public KeyValueProperties Properties { get; set; }

    }
}