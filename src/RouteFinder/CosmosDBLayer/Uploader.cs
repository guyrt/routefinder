namespace CosmosDBLayer
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
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

        public async Task<UserSummary> GetUserSummary(Guid userId)
        {
            using var lookup = await _container.ReadItemStreamAsync(userId.ToString(), new PartitionKey(userId.ToString()));
            if (lookup.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<UserSummary>(await new StreamReader(lookup.Content).ReadToEndAsync());
            }
            return null;
        }

        public async Task<List<UserWayCoverage>> GetAllUserWaySummaries(Guid userId)
        {
            var lookup = _container.GetItemLinqQueryable<UserWayCoverage>()
                            .Where(n => n.UserId == userId)
                            .Where(n => n.Type == "UserWayCoverage");

            using var feedIterator = lookup.ToFeedIterator();
            var nodes = new List<UserWayCoverage>();
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

        public async Task UploadGroupAsync<T>(IEnumerable<T> entities)
            where T : IPartitionedDataModel
        {
            var tasks = entities.Select(x => _container.UpsertItemAsync(x, new PartitionKey(x.UserId.ToString())));
            await Task.WhenAll(tasks);
        }

        public async Task UploadToDefaultPartition<T>(IEnumerable<T> entities, string partition)
        {
            var tasks = entities.Select(x => _container.UpsertItemAsync(x, new PartitionKey(partition)));
            await Task.WhenAll(tasks);
        }

        public async Task Upload<T>(T item)
            where T : IPartitionedDataModel
        {
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
