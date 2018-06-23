namespace Orleans.StorageProviders
{
    internal enum ProviderErrorCode
    {
        RedisProviderBase = 300000,
        RedisStorageproviderProviderName = RedisProviderBase + 200,
        RedisStorageProviderReadingData = RedisProviderBase + 300,
        RedisStorageProviderWritingData = RedisProviderBase + 400,
        RedisStorageProviderClearingData = RedisProviderBase + 500
    }
}
