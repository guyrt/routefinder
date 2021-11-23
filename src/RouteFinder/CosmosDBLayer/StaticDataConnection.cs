using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserDataModel;

namespace CosmosDBLayer
{
    internal class StaticDataConnection : CosmosConnectionBase
    {
        public StaticDataConnection(CosmosClient cosmosClient, string database, string container) : base(cosmosClient, database, container)
        { }

        public async Task UploadToDefaultPartition<T>(IEnumerable<T> entities, string partition)
        {
            var tasks = entities.Select(x => container.UpsertItemAsync(x, new PartitionKey(partition)));
            await Task.WhenAll(tasks);
        }

        internal async Task<IEnumerable<RegionSummary>> GetRegionSummaries(IEnumerable<string> regions)
        {
            Console.WriteLine($"Getting RegionSummary based on {regions.Count()} regions");
            var lookup = container.GetItemLinqQueryable<RegionSummary>()
                            .Where(n => n.Type == "RegionSummary")
                            .Where(n => regions.Contains(n.RegionId));

            using var feedIterator = lookup.ToFeedIterator();
            var outputRegions = new List<RegionSummary>();
            while (feedIterator.HasMoreResults)
            {
                foreach (var item in await feedIterator.ReadNextAsync())
                {
                    outputRegions.Add(item);
                }
            }

            return outputRegions;
        }
    }
}
