using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDBLayer;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using GlobalSettings;
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

            (var userId, var rawGpxData) = context.GetInput<(Guid, string)>();
            var parsedGpx = GpxParser.Parse(new StringReader(rawGpxData));

            // todo - also upload the file to raw storage!
            // todo - and record the raw file location in the uploaded cache function below.

            var config = SettingsManager.GetCredentials(); // todo this doesn't work. probably just move these configs for Azure Fxn
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            var tripHandler = new TripProcessorHandler(cosmosWriter);
            var plusCodeRanges = TripProcessorHandler.GetPlusCodeRanges(parsedGpx);

            tripHandler = await context.CallActivityAsync<TripProcessorHandler>("GpxFileUpload_WarmCache", (parsedGpx, tripHandler));

            // parallel tracks: upload raw while also computing overlaps.
            var overlapComputeParams = (parsedGpx, userId, plusCodeRanges, tripHandler);
            var overlappingNodesTask = context.CallActivityAsync<HashSet<UserNodeCoverage>>("GpxFileUpload_OverlappingNodes", overlapComputeParams);
            
            var uploadRawTask = context.CallActivityAsync("GpxFileUpload_UploadRawRun", (parsedGpx, userId, tripHandler));
            Task.WaitAll(overlappingNodesTask, uploadRawTask);
            var overlappingNodes = overlappingNodesTask.Result;

            // upload overlapping nodes
            var uploadCacheTask = context.CallActivityAsync("GpxFileUpload_UploadCache", (overlappingNodes, tripHandler));
            var updateUserWayCoverage = context.CallActivityAsync("GpxFileUpload_UpdateUserWayCoverage", (overlappingNodes, userId, plusCodeRanges, tripHandler));
            Task.WaitAll(uploadCacheTask, updateUserWayCoverage);

            // upload summary
            await context.CallActivityAsync("GpxFileUpload_UpdateUserSummaryAsync", (userId, tripHandler));

            return userId;
        }

        [FunctionName("GpxFileUpload_WarmCache")]
        public static TripProcessorHandler WarmTripCache([ActivityTrigger] IDurableActivityContext inputs, ILogger log)
        {
            (var parsedGpx, var tripHandler) = inputs.GetInput<(gpxType, TripProcessorHandler)>();
            tripHandler.WarmCache(parsedGpx);
            return tripHandler;
        }

        [FunctionName("GpxFileUpload_OverlappingNodes")]
        public static HashSet<UserNodeCoverage> GetOverlap([ActivityTrigger] IDurableActivityContext inputs, ILogger log)
        {
            (var parsedGpx, var userId, var plusCodeRanges, var tripHandler) = inputs.GetInput<(gpxType, Guid, HashSet<string>, TripProcessorHandler)>();
            return tripHandler.GetOverlap(parsedGpx, userId, plusCodeRanges);
        }

        [FunctionName("GpxFileUpload_UploadCache")]
        public async static Task UploadCache([ActivityTrigger] IDurableActivityContext payload, ILogger log)
        {
            (var overlappingNodes, var tripHandler) = payload.GetInput<(HashSet<UserNodeCoverage>, TripProcessorHandler)>();
            await tripHandler.UploadRawCache(overlappingNodes);
        }

        [FunctionName("GpxFileUpload_UploadRawRun")]
        public static async Task UploadRawRun([ActivityTrigger] IDurableActivityContext payload, ILogger log)
        {
            (var parsedGpx, Guid userId, var tripHandler) = payload.GetInput<(gpxType, Guid, TripProcessorHandler tripHandler)>();
            await tripHandler.UploadRawRun(parsedGpx, userId);
        }

        [FunctionName("GpxFileUpload_UpdateUserWayCoverage")]
        public static async Task UpdateUserWayCoverage([ActivityTrigger] IDurableActivityContext inputs, ILogger log)
        {
            (var userNodeCoverages, var userId, var plusCodeRanges, var tripHandler) = inputs.GetInput<(HashSet<UserNodeCoverage>, Guid, HashSet<string>, TripProcessorHandler)>();
            await tripHandler.UpdateUserWayCoverage(userNodeCoverages, userId, plusCodeRanges);
        }

        [FunctionName("GpxFileUpload_UpdateUserSummaryAsync")]
        public static async Task UpdateUserSummaryAsync([ActivityTrigger] IDurableActivityContext inputs, ILogger log)
        {
            (var userId, var tripHandler) = inputs.GetInput<(Guid, TripProcessorHandler)>();
            await tripHandler.UpdateUserSummaryAsync(userId);
    }


        [FunctionName("GpxFileUpload_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "gpx/{userId}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var data = await new StreamReader(req.Content.ReadAsStream()).ReadToEndAsync();
            var uri = req.RequestUri.ToString();
            var userIdStr = uri.Split("/").Last();
            var userId = Guid.Parse(userIdStr);

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync<(Guid UserId, string Payload)>("GpxFileUpload", null, (userId, data));

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}