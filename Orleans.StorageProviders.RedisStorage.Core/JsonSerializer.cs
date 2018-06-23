using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProtoBuf;

namespace Orleans.StorageProviders
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings; 

        public JsonSerializer(JsonSerializerSettings settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            };
        }

        public string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, _settings);
        }

        public object Deserialize(string data)
        {
            return JsonConvert.DeserializeObject(data, _settings);
        }
    }

    public class ProtoSerialuzer : ISerializer
    {

        public string Serialize(object data)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, data);

            return Convert.ToBase64String(stream.ToArray());
        }

        public object Deserialize(string data)
        {
            var stream = new MemoryStream(Convert.FromBase64String(data));
            return Serializer.Deserialize<IDictionary<string, object>>(stream);
        }
    }
}