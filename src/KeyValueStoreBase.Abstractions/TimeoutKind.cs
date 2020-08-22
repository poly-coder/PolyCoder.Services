namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    public enum TimeoutKind
    {
        None = 0,
        Default = 1,
        Date = 2,
        TimespanMinutes = 3,
        Permanent = 4,
    }
}