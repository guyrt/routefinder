using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureBlobHandler;
using CosmosDBLayer;
using Google.Protobuf;
using Newtonsoft.Json;
using RouteCleaner;
using RouteFinderDataModel.Proto;
using UserDataModel;

namespace GlobalSettings
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            // temporary link to a single box - this one happens to contain all of washington.
            var tmpRemoteBoundaries = "rawxml/boundaries.xml";
            var tmpRemoteRunnableWays = "rawxml/bbox_1_1.xml";

            Console.WriteLine($"Processing remote file {tmpRemoteRunnableWays}");

            var config = SettingsManager.GetCredentials();
            var rawDataDownloader = new DataDownloadWrapper(config.AzureRawXmlDownloadConnectionString, config.AzureRawXmlDownloadContainer);

            // pre build this to ensure connection is valid prior to entering expensive compute step.
            var rawDataUploader = new RawDataUploader(config.AzureRawXmlDownloadConnectionString, config.AzureBlobProcessedNodesContainer);

            await rawDataDownloader.RetrieveBlobAsync(tmpRemoteBoundaries, "/tmp/boundaries.xml");
            await rawDataDownloader.RetrieveBlobAsync(tmpRemoteRunnableWays, "/tmp/runnableWays.xml");

            // write temporary files with nodes and all targetable ways
            new RouteFinderDataPrepDriver().RunChain("/tmp/boundaries.xml", "/tmp/runnableWays.xml");

            // separate ways into sections
            await new NodeContainingWaysDriver().ProcessNodesAsync();

            // todo this saves targetable ways in bulk, but we need to save them on a smaller scale like we do nodes.
            if (RouteCleanerSettings.GetInstance().ShouldUploadRawTargetableWays)
            {
                var wayContents = File.ReadAllBytes(RouteCleanerSettings.GetInstance().TemporaryTargetableWaysLocation);
                await rawDataUploader.WriteBlobAsync("ways/targetableWays.json", wayContents);
            }

            SaveProtobufsToAzure(rawDataUploader);
            SaveWayProtobufsToAzure(rawDataUploader);
            await SaveRegionSummariesToAzureAsync(rawDataUploader);
        }

        private static void SaveProtobufsToAzure(RawDataUploader rawDataUploader)
        {
            // search in the right directory and discover all files. Create dir from remoteRunnableWays then upload all files there.
            Parallel.ForEach(ProtobufAreaConverter.CreateLookupNodeProtobufs(),
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (row) =>
                {
                    var key = row.Item1;
                    var nodes = row.Item2;
                    var nodeFileObj = new FullNodeSet();
                    nodeFileObj.Nodes.AddRange(nodes);
                    Console.WriteLine($"{key}: prepped {nodes.Count} nodes: {nodeFileObj.CalculateSize()} bytes");
                    var byteArray = nodeFileObj.ToByteArray();

                    var fileName = $"nodes/{key.Substring(0, 2)}/{key}";
                    await rawDataUploader.WriteBlobAsync(fileName, byteArray);
                });
        }

        private static void SaveWayProtobufsToAzure(RawDataUploader rawDataUploader)
        {
            Parallel.ForEach(ProtobufAreaConverter.CreateWayProtobufs(),
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (kvp) =>
                {
                    var ways = kvp.Value;
                    var key = kvp.Key;
                    var nodeFileObj = new FullWaySet();
                    nodeFileObj.Ways.AddRange(ways);
                    Console.WriteLine($"{key}: prepped {ways.Count} ways: {nodeFileObj.CalculateSize()} bytes");
                    var byteArray = nodeFileObj.ToByteArray();

                    var s = FullWaySet.Parser.ParseFrom(byteArray);

                    var fileName = $"ways/{key[..2]}/{key}";
                    await rawDataUploader.WriteBlobAsync(fileName, byteArray);
                });
        }

        private static async Task SaveRegionSummariesToAzureAsync(RawDataUploader rawDataUploader)
        {
            var watch = Stopwatch.StartNew();
            watch.Start();
            var settings = RouteCleanerSettings.GetInstance();
            await rawDataUploader.UploadFileAsync(settings.RemoteRelationSummaryLocation, settings.TemporaryRelationSummaryLocation);

            var regionSummaries = File.ReadLines(settings.TemporaryRelationSummaryLocation).Select(x => JsonConvert.DeserializeObject<RegionSummary>(x));

            var config = SettingsManager.GetCredentials();
            var cosmosWriter = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);
            await cosmosWriter.UploadToDefaultPartition(regionSummaries, Guid.Empty.ToString());

            var time = watch.Elapsed;
            Console.WriteLine($"Uploaded region summaries in {time}");
        }
    }
}
