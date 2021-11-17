namespace CosmosDBLayer
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public async Task<List<T>> GetAllDocumentsByWay<T>(Guid userId, string type, IEnumerable<string> wayIds)
            where T : IPartitionedWithWay
        {
            Console.WriteLine($"Getting {type} based on {wayIds.Count()} ways");
            var lookup = _container.GetItemLinqQueryable<T>()
                            .Where(n => n.UserId == userId)
                            .Where(n => n.Type == type)
                            .Where(n => wayIds.Contains(n.WayId));  // todo - validate this is expressed in efficient IN query.
            
            using var feedIterator = lookup.ToFeedIterator();
            var nodes = new List<T>();
            while (feedIterator.HasMoreResults)
            {
                foreach (var item in await feedIterator.ReadNextAsync())
                {
                    nodes.Add(item);
                }
            }

            return nodes;
        }

        internal Task<List<UserNodeCoverage>> GetAllUserNodeTasks(Guid userId, (string RegionId, string WayId)[] uniqueRegionWays)
        {
            throw new NotImplementedException();
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

        public async Task UploadWithDeleteAsync<T>(T item)
            where T : IPartitionedDataModel
        {
        //    await _container.DeleteItemStreamAsync(item.Id, new PartitionKey(item.UserId.ToString()));
            var response = await _container.UpsertItemAsync(item);
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
