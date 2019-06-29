using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using OpenStreetMapEtl;
using OpenStreetMapEtl.Utils;

namespace ProcessRawOsmBoundingBox
{
    public static class Function1
    {
        [FunctionName("ProcessRawOsmBoundingBox")]
        public static async void Run([TimerTrigger("0 */4 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var boundingBox = await GetMessage();

            try
            {
                var downloader = new DownloaderGeneral(new OsmDownloader());
                var geometry = downloader.RunSingleSquare(boundingBox);
                downloader.SaveGeometry(geometry, boundingBox);
                log.LogInformation($"Saved {boundingBox}");
            }
            catch (Exception ex)
            {
                log.LogInformation($"Exception: {ex}");
                Requeue(boundingBox);
            }
        }

        private static CloudQueue GetQueue()
        {
            // Retrieve storage account from connection string.
            // todo fix
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("");

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("bounding-box-downloadrequests");

            return queue;
        }

        private static async Task<BoundingBox> GetMessage()
        {
            var queue = GetQueue();
            // Create a message and add it to the queue.
            CloudQueueMessage message = await queue.GetMessageAsync();

            return JsonConvert.DeserializeObject<BoundingBox>(message.AsString);
        }

        private static async void Requeue(BoundingBox bbox)
        {
            var queue = GetQueue();
            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(bbox)));
        }
    }
}
