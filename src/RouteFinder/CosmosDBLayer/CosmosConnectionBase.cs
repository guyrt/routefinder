using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace CosmosDBLayer
{
    abstract internal class CosmosConnectionBase
    {
        internal Container container;
        private readonly CosmosClient _cosmosClient;

        private readonly string _databaseName;
        private readonly string _containerName;

        private bool initialized;

        public CosmosConnectionBase(CosmosClient cosmosClient, string database, string container)
        {
            _cosmosClient = cosmosClient;
            _databaseName = database;
            _containerName = container;

            initialized = false;
        }

        public async Task Initialize()
        {
            if (initialized)
            {
                return;
            }
            var database = (await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName)).Database;

            container = database.GetContainer(_containerName);

            initialized = true;
        }
    }
}
