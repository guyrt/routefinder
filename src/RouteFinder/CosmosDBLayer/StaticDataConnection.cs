using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
