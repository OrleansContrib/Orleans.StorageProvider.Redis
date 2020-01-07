using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Storage;

namespace Orleans.StorageProviders.RedisStorage
{
    public static class RedisGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<RedisStorageOptions>>();
            return ActivatorUtilities.CreateInstance<RedisGrainStorage>(services, name, optionsMonitor.Get(name));
        }
    }
}