namespace CosmosDBLayer
{
    using Microsoft.Azure.Cosmos;
    using RouteFinderDataModel;
    using System;
    using System.Threading.Tasks;
    using UserDataModel;

    public class Uploader
    {

        private Container _container;
        private readonly CosmosClient _cosmosClient;

        private readonly string _databaseName;
        private readonly string _containerName;

        private bool initialized;

        public Uploader(CosmosClient cosmosClient, string database, string container)
        {
            _cosmosClient = cosmosClient;
            _databaseName = database;
            _containerName = container;

            initialized = false;
        }

        /// <summary>
        /// Upload but only if item doesn't already exist. This keeps first timestamp.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public async Task UploadIfNotExistsAsync(UserNodeCoverage node)
        {
            using var lookup = await _container.ReadItemStreamAsync(node.Id, new PartitionKey(node.UserId.ToString()));
            if (lookup.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var response = await _container.UpsertItemAsync(node);
                Console.WriteLine(response.StatusCode);
            }
        }

        public async Task UploadAsync<T>(T way)
        {
            var response = await _container.UpsertItemAsync(way);
            Console.WriteLine(response.StatusCode);
        }

        public async Task Initialize()
        {
            if (initialized)
            {
                return;
            }
            var database = (await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName)).Database;

            _container = database.GetContainer(_containerName);

            initialized = true;
        }

    }
}
