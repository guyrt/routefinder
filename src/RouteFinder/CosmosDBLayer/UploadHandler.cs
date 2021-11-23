namespace CosmosDBLayer
{
    using Microsoft.Azure.Cosmos;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UserDataModel;

    public class UploadHandler
    {
        private readonly string _endpoint;

        private readonly string _authKey;

        private readonly string _databaseName;

        private readonly CosmosClient cosmosClient;

        private readonly UserHistoryConnection userHistory;

        private readonly StaticDataConnection staticData;

        public UploadHandler(string endPoint, string authKey, string database, string userHistoryContainer, string staticDataContainer)
        {
            _endpoint = endPoint;
            _authKey = authKey;
            _databaseName = database;

            // test connect
            CosmosClientOptions options = new CosmosClientOptions() { AllowBulkExecution = true };
            cosmosClient = new CosmosClient(_endpoint, _authKey, options);
            userHistory = new UserHistoryConnection(cosmosClient, _databaseName, userHistoryContainer);
            staticData = new StaticDataConnection(cosmosClient, _databaseName, staticDataContainer);
        }

        public async Task<List<UserNodeCoverage>> GetAllUserNodeCoverageByWay(Guid userId, IEnumerable<string> uniqueWays)
        {
            await userHistory.Initialize();
            return await userHistory.GetAllDocumentsByWay<UserNodeCoverage>(userId, "UserNodeCoverage", uniqueWays);
        }

        public async Task<List<UserWayCoverage>> GetAllUserWayCoverage(Guid userId, IEnumerable<string> uniqueWays)
        {
            await userHistory.Initialize();
            return await userHistory.GetAllDocumentsByWay<UserWayCoverage>(userId, "UserWayCoverage", uniqueWays);
        }

        public async Task<UserSummary> GetUserSummary(Guid userId)
        {
            await userHistory.Initialize();
            return await userHistory.GetUserSummary(userId);
        }

        public async Task<List<UserWayCoverage>> GetAllUserWaySummaries(Guid userId)
        {
            await userHistory.Initialize();
            return await userHistory.GetAllUserWaySummaries(userId);
        }

        public async Task Upload<T>(IEnumerable<T> runDetails)
            where T : IUserIdPartitionedDataModel
        {
            await userHistory.Initialize();
            await userHistory.UploadGroupAsync(runDetails);

            Console.WriteLine($"Uploaded {runDetails.Count()} of {runDetails.GetType().GetGenericArguments()[0].GetType()}");
        }

        public async Task UploadToDefaultPartition<T>(IEnumerable<T> entities, string partition)
        {
            await staticData.Initialize();
            await staticData.UploadToDefaultPartition(entities, partition);
        }

        public async Task Upload<T>(T runDetails)
            where T : IUserIdPartitionedDataModel
        {
            await userHistory.Initialize();
            await userHistory.Upload(runDetails);

            Console.WriteLine($"Uploaded {runDetails.GetType()} {runDetails}");
        }
    }
}
