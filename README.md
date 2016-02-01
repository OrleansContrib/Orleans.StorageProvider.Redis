# Orleans.StorageProvider.Redis

[![Build status](https://ci.appveyor.com/api/projects/status/x7j20s8bdhm7tkvq?svg=true)](https://ci.appveyor.com/project/richorama/orleans-storageprovider-redis)

A Redis implementation of the Orleans Storage Provider model. Uses the Azure Redis Cache to persist grain states.

Decorate your grain with the right attribute e.g.

```cs
[StorageProvider(ProviderName = "RedisStore")]
```

and in your OrleansConfiguration.xml configure the RedisStorage provider like this:

```xml
<Provider Type="Orleans.StorageProviders.RedisStorage" Name="RedisStore"
    RedisConnectionString="<youraccount>.redis.cache.windows.net,abortConnect=false,ssl=true,password=<yourkey>"/>
```

These settings will enable the redis cache to act as the store for grains that have 
a) state
b) need to persist their state
