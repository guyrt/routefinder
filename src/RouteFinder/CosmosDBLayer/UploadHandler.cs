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
    using UserDataModel;

    public class UploadHandler
    {
        private readonly string _endpoint;

        private readonly string _authKey;

        private readonly string _databaseName;

        private readonly string _containerName;

        private readonly CosmosClient cosmosClient;

        private readonly Uploader uploader;

        public UploadHandler(string endPoint, string authKey, string database, string container)
        {
            _endpoint = endPoint;
            _authKey = authKey;
            _databaseName = database;
            _containerName = container;

            // test connect
            cosmosClient = new CosmosClient(_endpoint, _authKey);
            uploader = new Uploader(cosmosClient, _databaseName, _containerName);
        }

        public async Task Upload(UserNodeCoverage userNode)
        {
            await uploader.Initialize();
            await uploader.UploadAsync(userNode);
            Console.WriteLine($"Uploaded node {userNode}");
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
                    await uploader.UploadAsync(way);
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
