using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.StorageProviders.RedisStorageCore;
using StackExchange.Redis;

namespace Orleans.StorageProviders
{
    public class RedisGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private ConnectionMultiplexer _connectionMultiplexer;
        private IDatabase _redisDatabase;
        private readonly ISerializer _serializer;
        private readonly ConfigurationOptions _redisOptions;

        /// <summary> Logger used by this storage provider instance. </summary>
        /// <see cref="IStorageProvider#Log"/>
        public ILogger Log { get; private set; }

        public RedisGrainStorage(ILogger<RedisGrainStorage> logger, ConfigurationOptions redisOptions)
        {
            Log = logger;
            _redisOptions = redisOptions;
            _serializer = new JsonSerializer();
        }

        /// <summary> Read state data function for this storage provider. </summary>
        /// <see cref="IStorageProvider#ReadStateAsync"/>
        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var primaryKey = grainReference.ToKeyString();

            if (Log.IsEnabled(LogLevel.Trace))
            {
                Log.Trace((int)ProviderErrorCode.RedisStorageProviderReadingData, "Reading: GrainType={0} Pk={1} Grainid={2} from Database={3}",
                    grainType, primaryKey, grainReference, _redisDatabase.Database);
            }

            RedisValue value = await _redisDatabase.StringGetAsync(primaryKey);
            if (value.HasValue)
            {
                grainState.State = _serializer.Deserialize(value);
            }
        }

        /// <summary> Write state data function for this storage provider. </summary>
        /// <see cref="IStorageProvider#WriteStateAsync"/>
        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var primaryKey = grainReference.ToKeyString();
            if (Log.IsEnabled(LogLevel.Trace))
            {
                Log.Trace((int)ProviderErrorCode.RedisStorageProviderWritingData, "Writing: GrainType={0} PrimaryKey={1} Grainid={2} ETag={3} to Database={4}",
                    grainType, primaryKey, grainReference, grainState.ETag, _redisDatabase.Database);
            }
            var data = _serializer.Serialize(grainState.State);
            await _redisDatabase.StringSetAsync(primaryKey, data);

        }

        /// <summary> Clear state data function for this storage provider. </summary>
        /// <remarks>
        /// </remarks>
        /// <see cref="IStorageProvider#ClearStateAsync"/>
        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var primaryKey = grainReference.ToKeyString();
            if (Log.IsEnabled(LogLevel.Trace))
            {
                Log.Trace((int)ProviderErrorCode.RedisStorageProviderClearingData, "Clearing: GrainType={0} Pk={1} Grainid={2} ETag={3} to Database={4}",
                    grainType, primaryKey, grainReference, grainState.ETag, _redisDatabase.Database);
            }
            return _redisDatabase.KeyDeleteAsync(primaryKey);
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            async Task Init(CancellationToken token)
            {
                _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_redisOptions);
                _redisDatabase = _connectionMultiplexer.GetDatabase();
            }
            lifecycle.Subscribe<RedisGrainStorage>(ServiceLifecycleStage.ApplicationServices, Init);
        }
    }
}
