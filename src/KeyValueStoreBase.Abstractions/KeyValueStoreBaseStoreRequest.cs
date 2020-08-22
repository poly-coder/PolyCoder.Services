using System;
using System.Collections.Generic;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueStoreBaseStoreRequest
    {
        public string Key { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public byte[] Value { get; set; }
        public string ContentType { get; set; }
        public TimeoutKind TimeoutKind { get; set; }
        public DateTimeOffset? TimeoutDate { get; set; }
        public TimeSpan? TimeoutSpan { get; set; }
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
    }
}