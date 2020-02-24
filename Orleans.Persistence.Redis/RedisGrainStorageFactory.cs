using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Storage;
using System;

namespace Orleans.Persistence
{
    public static class RedisGrainStorageFactory
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            IOptionsMonitor<RedisStorageOptions> optionsSnapshot = services.GetRequiredService<IOptionsMonitor<RedisStorageOptions>>();
            return ActivatorUtilities.CreateInstance<RedisGrainStorage>(services, optionsSnapshot.Get(name), name);
        }
    }
}
