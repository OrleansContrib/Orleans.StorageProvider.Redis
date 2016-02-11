# Orleans.StorageProvider.Redis

[![Build status](https://ci.appveyor.com/api/projects/status/x7j20s8bdhm7tkvq?svg=true)](https://ci.appveyor.com/project/richorama/orleans-storageprovider-redis)

A Redis implementation of the Orleans Storage Provider model. Uses the Azure Redis Cache to persist grain states.

## Usage

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

* State
* Need to persist their state

## Configuration

The following attributes can be used on the `<Provider/>` tag to configure the provider:

* __UseJsonFormat="true/false"__ (optional) Defaults to `true`, if set to `false` the Orleans binary serializer is used (this is recommended, as the JSON serializer is unable to serialize certain types).
* __RedisConnectionString="..."__ (required) the connection string to your redis database (i.e. `<youraccount>.redis.cache.windows.net,abortConnect=false,ssl=true,password=<yourkey>`)
* __DatabaseNumber="1"__ (optional) the number of the redis database to connect to
