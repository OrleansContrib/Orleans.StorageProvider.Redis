using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using StackExchange.Redis;

namespace Orleans.StorageProviders.RedisStorage
{
    /// <summary>
    /// Simple storage provider for writing grain state data to redis storage in JSON format.
    /// </summary>
    public class RedisGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private ConnectionMultiplexer _connection;
        private IDatabase _db;

        private const string WriteScript = "local etag = redis.call('HGET', @key, 'etag')\nif etag == false or etag == @etag then return redis.call('HMSET', @key, 'etag', @newEtag, 'data', @data) else return false end";

        private readonly string _serviceId;
        private readonly string _name;
        private readonly SerializationManager _serializationManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly RedisStorageOptions _options;

        private JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        private ConfigurationOptions _redisOptions;
        private LuaScript _preparedWriteScript;

        /// <summary> Default constructor </summary>
        public RedisGrainStorage(
            string name,
            RedisStorageOptions options,
            SerializationManager serializationManager,
            IGrainFactory grainFactory,
            ITypeResolver typeResolver,
            IOptions<ClusterOptions> clusterOptions,
            ILoggerFactory loggerFactory)
        {
            _name = name;

            _loggerFactory = loggerFactory;
            var loggerName = $"{typeof(RedisGrainStorage).FullName}.{name}";
            _logger = loggerFactory.CreateLogger(loggerName);

            _options = options;
            _serializationManager = serializationManager;
            _serviceId = clusterOptions.Value.ServiceId;
        }


        public void Participate(ISiloLifecycle lifecycle)
        {
            var name = OptionFormattingUtilities.Name<RedisGrainStorage>(_name);
            lifecycle.Subscribe(name, _options.InitStage, Init, Close);
        }

        /// <summary> Initialization function for this storage provider. </summary>
        /// <see cref="IProvider#Init"/>
        public async Task Init(CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();

            try
            {
                var initMsg = $"Init: Name={_name} ServiceId={_serviceId} DatabaseNumber={_options.DatabaseNumber} UseJson={_options.UseJson} DeleteOnClear={_options.DeleteOnClear}";
                _logger.LogInformation($"RedisGrainStorage {_name} is initializing: {initMsg}");
                
                _redisOptions = ConfigurationOptions.Parse(_options.DataConnectionString);
                _redisOptions.ConnectRetry = _options.NumberOfConnectionRetries;
                _redisOptions.ReconnectRetryPolicy = new ExponentialRetry(_options.DeltaBackOffMilliseconds);
                _connection = await ConnectionMultiplexer.ConnectAsync(_redisOptions);
                
                if (_options.DatabaseNumber.HasValue)
                    _db = _connection.GetDatabase(_options.DatabaseNumber.Value);
                else
                    _db = _connection.GetDatabase();

                _preparedWriteScript = LuaScript.Prepare(WriteScript);

                var loadTasks = new Task[_redisOptions.EndPoints.Count];
                for (int i = 0; i < _redisOptions.EndPoints.Count; i++)
                {
                    var endpoint = _redisOptions.EndPoints.ElementAt(i);
                    var server = _connection.GetServer(endpoint);

                    loadTasks[i] = _preparedWriteScript.LoadAsync(server);
                }
                await Task.WhenAll(loadTasks);

                timer.Stop();
                _logger.LogInformation("Init: Name={0} ServiceId={1}, initialized in {2} ms",
                    _name, _serviceId, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
            catch (Exception ex)
            {
                timer.Stop();
                _logger.LogError(ex, "Init: Name={0} ServiceId={1}, errored in {2} ms. Error message: {3}",
                    _name, _serviceId, timer.Elapsed.TotalMilliseconds.ToString("0.00"), ex.Message);
                throw ex;
            }
        }

        /// <summary> Read state data function for this storage provider. </summary>
        /// <see cref="IStorageProvider#ReadStateAsync"/>
        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var timer = Stopwatch.StartNew();

            var key = GetKey(grainReference);

            try
            {
                var hashEntries = await _db.HashGetAllAsync(key);
                if (hashEntries.Count() == 2)
                {
                    var etagEntry = hashEntries.Single(e => e.Name == "etag");
                    var valueEntry = hashEntries.Single(e => e.Name == "data");
                    if (_options.UseJson)
                        grainState.State = JsonConvert.DeserializeObject(valueEntry.Value, grainState.State.GetType(), JsonSettings);
                    else
                        grainState.State = _serializationManager.DeserializeFromByteArray<object>(valueEntry.Value);
                    grainState.ETag = etagEntry.Value;
                }
                else
                {
                    grainState.ETag = Guid.NewGuid().ToString();
                }
                timer.Stop();
                _logger.LogInformation("Reading: GrainType={0} Pk={1} Grainid={2} ETag={3} from Database={4}, finished in {5} ms",
                    grainType, key, grainReference, grainState.ETag, _db.Database, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
            catch (RedisServerException)
            {
                var stringValue = await _db.StringGetAsync(key);
                if (stringValue.HasValue)
                {
                    if (_options.UseJson)
                        grainState.State = JsonConvert.DeserializeObject(stringValue, grainState.State.GetType(), JsonSettings);
                    else
                        grainState.State = _serializationManager.DeserializeFromByteArray<object>(stringValue);
                }
                grainState.ETag = Guid.NewGuid().ToString();
                timer.Stop();
                _logger.LogInformation("Reading: GrainType={0} Pk={1} Grainid={2} ETag={3} from Database={4}, finished in {5} ms (migrated old Redis data, grain now supports ETag)",
                    grainType, key, grainReference, grainState.ETag, _db.Database, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
        }

        /// <summary> Write state data function for this storage provider. </summary>
        /// <see cref="IStorageProvider#WriteStateAsync"/>
        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var timer = Stopwatch.StartNew();
            var key = GetKey(grainReference);

            RedisResult response = null;
            var newEtag = Guid.NewGuid().ToString();
            if (_options.UseJson)
            {
                var payload = JsonConvert.SerializeObject(grainState.State, JsonSettings);
                var args = new { key, etag = grainState.ETag ?? "null", newEtag, data = payload };
                response = await _db.ScriptEvaluateAsync(_preparedWriteScript, args);
            }
            else
            {
                var payload = _serializationManager.SerializeToByteArray(grainState.State);
                var args = new { key, etag = grainState.ETag ?? "null", newEtag, data = payload };
                response = await _db.ScriptEvaluateAsync(_preparedWriteScript, args);
            }

            if (response.IsNull)
            {
                timer.Stop();
                _logger.LogError("Writing: GrainType={0} PrimaryKey={1} Grainid={2} ETag={3} to Database={4}, finished in {5} ms - Error: ETag mismatch!",
                    grainType, key, grainReference, grainState.ETag, _db.Database, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
                throw new InconsistentStateException($"ETag mismatch - tried with ETag: {grainState.ETag}");
            }

            grainState.ETag = newEtag;

            timer.Stop();
            _logger.LogInformation("Writing: GrainType={0} PrimaryKey={1} Grainid={2} ETag={3} to Database={4}, finished in {5} ms",
                grainType, key, grainReference, grainState.ETag, _db.Database, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
        }

        /// <summary> Clear state data function for this storage provider. </summary>
        /// <remarks>
        /// </remarks>
        /// <see cref="IStorageProvider#ClearStateAsync"/>
        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var timer = Stopwatch.StartNew();
            var key = GetKey(grainReference);

            await _db.KeyDeleteAsync(key);

            timer.Stop();
            _logger.LogInformation("Clearing: GrainType={0} Pk={1} Grainid={2} ETag={3} to Database={4}, finished in {5} ms",
                grainType, key, grainReference, grainState.ETag, _db.Database, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
        }

        private string GetKey(GrainReference grainReference)
        {
            var format = _options.UseJson ? "json" : "binary";
            return $"{grainReference.ToKeyString()}|{format}";
        }

        public Task Close(CancellationToken cancellationToken)
        {
            _connection.Dispose();
            return Task.CompletedTask;
        }
    }
}