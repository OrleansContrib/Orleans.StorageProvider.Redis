using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using StackExchange.Redis;

namespace Orleans.StorageProviders.RedisStorageCore
{
    public class RedisConnectionString
    {
        public string ConnectionString { get; set; }
    }

    public static class RedisSiloExtensions
    {
        /// <summary>
        /// Configure silo to use MongoDB as the default grain storage.
        /// </summary>
        public static ISiloHostBuilder AddRedisGrainStorageAsDefault(this ISiloHostBuilder builder,
            Action<ConfigurationOptions> configureOptions = null, ISerializer serializer = null)
        {
            var name = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;

            return builder.ConfigureServices(services => services
                .AddRedisGrainStorage(name, ob => ob.Configure(configureOptions), serializer));
        }

        private static void AddRedisGrainStorage(this IServiceCollection services, string name,
            Action<OptionsBuilder<ConfigurationOptions>> configureOptions = null, ISerializer serializer = null)
        {
            configureOptions?.Invoke(services.AddOptions<ConfigurationOptions>(name));
            
            services.AddSingleton(sp => sp.GetServiceByName<IGrainStorage>(name));

            services.AddSingletonNamedService(name, RedisGrainStorageFactory.Create);
            ILifecycleParticipant<ISiloLifecycle> Factory(IServiceProvider provider, string n)
            {
                return (ILifecycleParticipant<ISiloLifecycle>) provider.GetRequiredServiceByName<IGrainStorage>(n);
            }
            services.AddSingletonNamedService(name, Factory);
        }
    }
}
