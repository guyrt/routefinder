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
    public static class GpxFileUpload
    {
        [FunctionName("GpxFileUpload")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            var parsedGpx = GpxParser.Parse("file");
            var plusCodeRanges = TripProcessorHandler.GetPlusCodeRanges(parsedGpx);

            (gpxType ParsedGpx, Guid UserId, HashSet<string> PlusCodeRanges) foo = (ParsedGpx: parsedGpx, UserId: Guid.NewGuid(), PlusCodeRanges: plusCodeRanges);
            var overlappingNodes = await context.CallActivityAsync<HashSet<UserNodeCoverage>>("GpxFileUpload_OverlappingNodes", foo);

            outputs.Add(await context.CallActivityAsync<string>("GpxFileUpload_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("GpxFileUpload_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("GpxFileUpload_OverlappingNodes")]
        public static HashSet<UserNodeCoverage> GetOverlap([ActivityTrigger] (gpxType ParsedGpx, Guid UserId, HashSet<string> PlusCodeRanges) payload, ILogger log)
        {
            var config = SettingsManager.GetCredentials();
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            var tripHandler = new TripProcessorHandler(cosmosWriter);
            var overlappingNodes = tripHandler.GetOverlap(payload.ParsedGpx, payload.UserId, payload.PlusCodeRanges);
            return overlappingNodes;
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