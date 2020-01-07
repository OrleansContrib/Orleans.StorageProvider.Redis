using System;
using Newtonsoft.Json;

namespace Orleans.StorageProviders.RedisStorage
{
    public class RedisStorageOptions
    {
        public string DataConnectionString { get; set; } = "localhost:6379";

        public bool DeleteOnClear { get; set; }

        public int? DatabaseNumber { get; set; }

        /// <summary>
        /// Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
        /// </summary>
        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;

        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;

        public bool UseJson { get; set; }
        public bool UseFullAssemblyNames { get; set; }
        public bool IndentJson { get; set; }
        public TypeNameHandling? TypeNameHandling { get; set; }
        public Action<JsonSerializerSettings> ConfigureJsonSerializerSettings { get; set; }
        
        public int NumberOfConnectionRetries { get; set; }
        public int DeltaBackOffMilliseconds { get; set; }
    }
}
