using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Storage;
using StackExchange.Redis;

namespace Orleans.StorageProviders
{
    /// <summary>
    /// Factory for creating MemoryGrainStorage
    /// </summary>
    public class RedisGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<ConfigurationOptions>>();
            return ActivatorUtilities.CreateInstance<RedisGrainStorage>(services, optionsSnapshot.Get(name));
        }
    }
}