# Orleans.StorageProvider.Redis
A Redis implementation of the Orleans Storage Provider model. Uses the Azure Redis Cache to persist grain states.

Decorate your grain with the right attribute e.g.

    [StorageProvider(ProviderName = "RedisStore")]

and in your OrleansConfiguration.xml configure the RedisStorage provider like this:

<StorageProviders>
      <Provider Type="Orleans.StorageProviders.RedisStorage" Name="RedisStore"
                RedisConnectionString="<youraccount>.redis.cache.windows.net,abortConnect=false,ssl=true,password=<yourkey>"/>
</StorageProviders>
