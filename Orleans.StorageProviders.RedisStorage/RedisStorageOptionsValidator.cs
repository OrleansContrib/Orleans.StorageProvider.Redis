using Orleans.Runtime;

namespace Orleans.StorageProviders.RedisStorage
{
    /// <summary>
    /// Configuration validator
    /// </summary>
    public class RedisStorageOptionsValidator : IConfigurationValidator
    {
        private readonly RedisStorageOptions _options;
        private readonly string _name;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">The option to be validated.</param>
        /// <param name="name">The option name to be validated.</param>
        public RedisStorageOptionsValidator(RedisStorageOptions options, string name)
        {
            this._options = options;
            this._name = name;
        }

        public void ValidateConfiguration()
        {
            var msg = $"Configuration for {nameof(RedisGrainStorage)} - {_name} is invalid";
            if (_options == null)
                throw new OrleansConfigurationException($"{msg} - {nameof(RedisStorageOptions)} is null");
            if (string.IsNullOrWhiteSpace(_options.DataConnectionString))
                throw new OrleansConfigurationException($"{msg} - {nameof(_options.DataConnectionString)} is null or empty");

            // host:port delimiter
            if (!_options.DataConnectionString.Contains(":"))
                throw new OrleansConfigurationException($"{msg} - {nameof(_options.DataConnectionString)} invalid format: {_options.DataConnectionString}, should contain host and port delimited by ':'");
        }
    }
}
