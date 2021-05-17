using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace CosmosDBLayer.Serializers
{
    public class CustomOsmSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        private readonly JsonSerializer _serializer;

        public CustomOsmSerializer(JsonSerializerSettings settings)
        {
            _serializer = JsonSerializer.Create(settings);
        }
        
        public override T FromStream<T>(Stream stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            using (var sr = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            var serialized = JsonConvert.SerializeObject(input);
            using (var sw = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
            {
                sw.Write(serialized);
            }
            return streamPayload;
        }
    }
}
