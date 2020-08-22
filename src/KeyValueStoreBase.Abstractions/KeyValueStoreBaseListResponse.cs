using System.Collections.Generic;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueStoreBaseListResponse
    {
        public Status Status { get; set; }
        public int PageNumber { get; set; }
        public List<KeyValueContent> Items { get; set; }
        public bool IsLastPage { get; set; }
    }
}