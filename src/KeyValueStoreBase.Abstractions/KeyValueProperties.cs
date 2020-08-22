using System;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public class KeyValueProperties
    {
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset? TimeToLive { get; set; }
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
    }
}