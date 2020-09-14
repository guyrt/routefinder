namespace CosmosDBLayer
{
    using Microsoft.Azure.Cosmos;
    using RouteFinderDataModel;
    using System.Threading.Tasks;

    public class Uploader
    {

        private Container _container;
        private readonly CosmosClient _cosmosClient;

        private readonly string _databaseName;
        private readonly string _containerName;

        public Uploader(CosmosClient cosmosClient, string database, string container)
        {
            _cosmosClient = cosmosClient;
            _databaseName = database;
            _containerName = container;
        }

        public async Task Upload(Way way)
        {
            await _container.CreateItemAsync(way);
        }

        public async Task Initialize()
        {
            var database = (await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName)).Database;

            _container = database.GetContainer(_containerName);
        }

    }
}
