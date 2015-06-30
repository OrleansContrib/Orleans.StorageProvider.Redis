# Orleans.StorageProvider.Redis
A Redis implementation of the Orleans Storage Provider model. Uses the Azure Redis Cache to persist grain states.

Decorate your grain with the right attribute e.g.

    [StorageProvider(ProviderName = "RedisStore")]

and in your OrleansConfiguration.xml configure the RedisStorage provider like this:

      <Provider Type="Orleans.StorageProviders.RedisStorage" Name="RedisStore"
                RedisConnectionString="<youraccount>.redis.cache.windows.net,abortConnect=false,ssl=true,password=<yourkey>"/>

These settings will enable the redis cache to act as the store for grains that have 
a) state
b) need to persist their state
