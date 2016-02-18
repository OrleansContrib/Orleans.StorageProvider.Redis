# Orleans.StorageProvider.Redis

[![Build status](https://ci.appveyor.com/api/projects/status/6xxnvi7rh131c9f1?svg=true)](https://ci.appveyor.com/project/OrleansContrib/orleans-storageprovider-redis)
dev branch
[![Build status](https://ci.appveyor.com/api/projects/status/6xxnvi7rh131c9f1/branch/dev?svg=true)](https://ci.appveyor.com/project/OrleansContrib/orleans-storageprovider-redis/branch/dev)

A Redis implementation of the Orleans Storage Provider model. Uses the Azure Redis Cache to persist grain states.

## Usage

```ps
Install-Package RedisStorage
```


Decorate your grain with the StorageProvider attribute matching the name you added from config e.g.

```cs
[StorageProvider(ProviderName = "RedisStore")]
```

and in your OrleansConfiguration.xml configure the RedisStorage provider like this:

```xml
<Provider Type="Orleans.StorageProviders.RedisStorage" Name="RedisStore"
    RedisConnectionString="<youraccount>.redis.cache.windows.net,abortConnect=false,ssl=true,password=<yourkey>"/>
```

These settings will enable the redis cache to act as the store for grains that have 

* State
* Need to persist their state

## Configuration

The following attributes can be used on the `<Provider/>` tag to configure the provider:

* __UseJsonFormat="true/false"__ (optional) Defaults to `true`, if set to `false` the Orleans binary serializer is used (this is recommended, as the JSON serializer is unable to serialize certain types).
* __RedisConnectionString="..."__ (required) the connection string to your redis database (i.e. `<youraccount>.redis.cache.windows.net,abortConnect=false,ssl=true,password=<yourkey>`)
* __DatabaseNumber="1"__ (optional) the number of the redis database to connect to

## License

MIT
