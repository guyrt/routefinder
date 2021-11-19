using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDBLayer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using OsmETL;
using TripProcessor;
using TripProcessor.GpxData;
using UserDataModel;

namespace TripIngestion
{
    /// <summary>
    /// todo - rename this to process and make it blob triggered
    /// </summary>
    public static class GpxFileUpload
    {
        [FunctionName("GpxFileUpload")]
        public static async Task<Guid> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var userId = Guid.NewGuid();
            // context.getInput

            // todo - also upload the file to raw storage!
            // todo - and record the raw file location in the uploaded cache function below.

            var config = SettingsManager.GetCredentials();
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            var tripHandler = new TripProcessorHandler(cosmosWriter);

            // Replace "hello" with the name of your Durable Activity Function.
            var parsedGpx = GpxParser.Parse("file");
            var plusCodeRanges = TripProcessorHandler.GetPlusCodeRanges(parsedGpx);

            // todo - prewarm cache in tripHandler.

            // parallel tracks: upload raw while also computing overlaps.
            var overlapComputeParams = (parsedGpx, userId, plusCodeRanges, tripHandler);
            var overlappingNodesTask = context.CallActivityAsync<HashSet<UserNodeCoverage>>("GpxFileUpload_OverlappingNodes", overlapComputeParams);
            
            var uploadRawTask = context.CallActivityAsync("GpxFileUpload_UploadRawRun", (ParsedGpx: parsedGpx, UserId: userId));
            Task.WaitAll(overlappingNodesTask, uploadRawTask);
            var overlappingNodes = overlappingNodesTask.Result;

            // upload overlapping nodes
            var uploadCacheTask = context.CallActivityAsync("GpxFileUpload_UploadCache", overlappingNodes);
            var updateUserWayCoverage = context.CallActivityAsync("GpxFileUpload_UpdateUserWayCoverage", (UserNodeCoverages: overlappingNodesTask, UserId: userId, PlusCodeRanges: plusCodeRanges));
            Task.WaitAll(uploadCacheTask, updateUserWayCoverage);

            return userId;
        }

        [FunctionName("GpxFileUpload_OverlappingNodes")]
        public static HashSet<UserNodeCoverage> GetOverlap([ActivityTrigger] IDurableActivityContext inputs, ILogger log)
        {
            (gpxType parsedGpx, Guid userId, HashSet<string> plusCodeRanges, TripProcessorHandler tripHandler) = inputs.GetInput<(gpxType, Guid, HashSet<string>, TripProcessorHandler)>();
            var overlappingNodes = tripHandler.GetOverlap(parsedGpx, userId, plusCodeRanges);
            return overlappingNodes;
        }

        [FunctionName("GpxFileUpload_UploadCache")]
        public static HashSet<UserNodeCoverage> UploadCache([ActivityTrigger] (gpxType ParsedGpx, Guid UserId, HashSet<string> PlusCodeRanges) payload, ILogger log)
        {
            var config = SettingsManager.GetCredentials();
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            var tripHandler = new TripProcessorHandler(cosmosWriter);
            var overlappingNodes = tripHandler.GetOverlap(payload.ParsedGpx, payload.UserId, payload.PlusCodeRanges);
            return overlappingNodes;
        }

        [FunctionName("GpxFileUpload_UploadRawRun")]
        public static async Task UploadRawRun([ActivityTrigger] (gpxType ParsedGpx, Guid UserId) payload, ILogger log)
        {
            var config = SettingsManager.GetCredentials();
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            var tripHandler = new TripProcessorHandler(cosmosWriter);
            await tripHandler.UploadRawRun(payload.ParsedGpx, payload.UserId);
        }

        [FunctionName("GpxFileUpload_UpdateUserWayCoverage")]
        public static async Task UpdateUserWayCoverage([ActivityTrigger] (HashSet<UserNodeCoverage> UserNodeCoverages, Guid UserId, HashSet<string> PlusCodeRanges) payload, ILogger log)
        {
            var config = SettingsManager.GetCredentials();
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            var tripHandler = new TripProcessorHandler(cosmosWriter);
            await tripHandler.UpdateUserWayCoverage(payload.UserNodeCoverages, payload.UserId, payload.PlusCodeRanges);
        }

        [FunctionName("GpxFileUpload_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("GpxFileUpload", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}