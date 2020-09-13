namespace CosmosDBLayer
{
    using Microsoft.Azure.Cosmos;
    using RouteFinderDataModel;
    using System.Threading.Tasks;

    public class Uploader
    {

        private Container _container;
        private readonly CosmosClient _cosmosClient;

        public Uploader(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        public async Task Upload(Way way)
        {
            await _container.CreateItemAsync(way);
        }

        public async Task Initialize()
        {
            var database = (await _cosmosClient.CreateDatabaseIfNotExistsAsync("foo")).Database;

            _container = database.GetContainer("container");
        }

    }
}
