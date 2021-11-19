using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CosmosDBLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

            (var rawGpxData, var userId) = context.GetInput<(string, Guid)>();
            var parsedGpx = GpxParser.Parse(new StringReader(rawGpxData));

            // todo - also upload the file to raw storage!
            // todo - and record the raw file location in the uploaded cache function below.

            var config = SettingsManager.GetCredentials();
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            var tripHandler = new TripProcessorHandler(cosmosWriter);
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
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "/gpx/{userId}")] HttpRequest req,
            string userIdStr,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var data = await new StreamReader(req.Body).ReadToEndAsync();
            var userId = Guid.Parse(userIdStr);

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync<(Guid UserId, string Payload)>("GpxFileUpload", null, (userId, data));

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return new CreatedResult(string.Empty, instanceId);
        }
    }
}