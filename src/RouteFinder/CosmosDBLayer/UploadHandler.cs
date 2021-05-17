namespace CosmosDBLayer
{
    using CosmosDBLayer.Serializers;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
    using RouteFinderDataModel;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class UploadHandler
    {
        private readonly string _endpoint;

        private readonly string _authKey;

        private readonly string _databaseName;

        private readonly string _containerName;

        public UploadHandler(string endPoint, string authKey, string database, string container)
        {
            _endpoint = endPoint;
            _authKey = authKey;
            _databaseName = database;
            _containerName = container;

            // test connect
            using (var client = new CosmosClient(_endpoint, _authKey)) { }
        }

        public async Task Upload(IEnumerable<Way> ways)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                Converters =
                {
                    new StoredWaySerDe()
                }
            };
            var clientOptions = new CosmosClientOptions
            {
                Serializer = new CustomOsmSerializer(serializerSettings)
            };

            using (var client = new CosmosClient(_endpoint, _authKey, clientOptions))
            {
                var uploader = new Uploader(client, _databaseName, _containerName);
                await uploader.Initialize();

                long i = 0;
                foreach (var way in ways)
                {
                    await uploader.Upload(way);
                    i++;
                    if (i % 10000 == 0)
                    {
                        Console.WriteLine($"Wrote {i}");
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
