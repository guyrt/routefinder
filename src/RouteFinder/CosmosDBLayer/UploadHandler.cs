namespace CosmosDBLayer
{
    using CosmosDBLayer.Serializers;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
    using RouteFinderDataModel;
    using System.Collections.Generic;

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

        public async void Upload(IEnumerable<Way> ways)
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

                foreach (var way in ways)
                {
                    await uploader.Upload(way);
                }
            }
        }
    }
}
