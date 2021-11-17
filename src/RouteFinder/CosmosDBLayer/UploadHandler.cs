namespace CosmosDBLayer
{
    using CosmosDBLayer.Serializers;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
    using RouteFinderDataModel;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UserDataModel;

    public class UploadHandler
    {
        private readonly string _endpoint;

        private readonly string _authKey;

        private readonly string _databaseName;

        private readonly string _containerName;

        private readonly CosmosClient cosmosClient;

        private readonly Uploader uploader;

        public UploadHandler(string endPoint, string authKey, string database, string container)
        {
            _endpoint = endPoint;
            _authKey = authKey;
            _databaseName = database;
            _containerName = container;

            // test connect
            cosmosClient = new CosmosClient(_endpoint, _authKey);
            uploader = new Uploader(cosmosClient, _databaseName, _containerName);
        }

        public async Task<List<UserNodeCoverage>> GetAllUserNodeCoverageByWay(Guid userId, IEnumerable<string> uniqueWays)
        {
            await uploader.Initialize();
            return await uploader.GetAllDocumentsByWay<UserNodeCoverage>(userId, "UserNodeCoverage", uniqueWays);
        }

        public async Task<List<UserWayCoverage>> GetAllUserWayCoverage(Guid userId, IEnumerable<string> uniqueWays)
        {
            await uploader.Initialize();
            return await uploader.GetAllDocumentsByWay<UserWayCoverage>(userId, "UserWayCoverage", uniqueWays);
        }

        public async Task Upload(UserNodeCoverage userNode)
        {
            await uploader.Initialize();
            await uploader.UploadIfNotExistsAsync(userNode);
        }

        public async Task Upload<T>(T runDetails)
            where T : IPartitionedDataModel
        {
            await uploader.Initialize();
            await uploader.UploadWithDeleteAsync(runDetails);

            Console.WriteLine($"Uploaded {runDetails.GetType()} {runDetails}");
        }
    }
}
