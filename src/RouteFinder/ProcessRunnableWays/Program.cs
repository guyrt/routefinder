using System;
using System.IO;
using System.Threading.Tasks;
using AzureBlobHandler;
using GlobalSettings;
using Google.Protobuf;
using RouteCleaner;
using RouteFinderDataModel.Proto;

namespace OsmETL
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
            new NodeContainingWaysDriver().ProcessNodes();

            // todo this saves targetable ways in bulk, but we need to save them on a smaller scale like we do nodes.
            if (RouteCleanerSettings.GetInstance().ShouldUploadRawTargetableWays)
            {
                var wayContents = File.ReadAllBytes(RouteCleanerSettings.GetInstance().TemporaryTargetableWaysLocation);
                await rawDataUploader.WriteBlobAsync("ways/targetableWays.json", wayContents);
            }

            SaveProtobufsToAzure(rawDataUploader);
            SaveWayProtobufsToAzure(rawDataUploader);
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
    }
}
