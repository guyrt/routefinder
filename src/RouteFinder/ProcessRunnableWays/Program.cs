using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureBlobHandler;
using Google.OpenLocationCode;
using Google.Protobuf;
using Newtonsoft.Json;
using RouteCleaner;
using RouteFinderDataModel;
using RouteFinderDataModel.Proto;

namespace OsmETL
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var tmpRemoteBoundaries = "rawxml/boundaries.xml";
            var tmpRemoteRunnableWays = "rawxml/bbox_1_1.xml";

            Console.WriteLine($"Processing remote file {tmpRemoteRunnableWays}");

            var config = SettingsManager.GetCredentials();
            var rawDataDownloader = new DataDownloadWrapper(config.AzureRawXmlDownloadConnectionString, config.AzureRawXmlDownloadContainer);
            var rawDataUploader = new RawDataUploader(config.AzureRawXmlDownloadConnectionString, config.AzureBlobProcessedNodesContainer);

            await rawDataDownloader.RetrieveBlobAsync(tmpRemoteBoundaries, "/tmp/boundaries.xml");
            await rawDataDownloader.RetrieveBlobAsync(tmpRemoteRunnableWays, "/tmp/runnableWays.xml");

            new RouteFinderDataPrepDriver().RunChain("/tmp/boundaries.xml", "/tmp/runnableWays.xml");
            new NodeContainingWaysDriver().ProcessNodes();

            // search in the right directory and discover all files. Create dir from remoteRunnableWays then upload all files there.
            foreach ((var key, var nodes) in CreateProtobufs())
            {
                var nodeFileObj = new FullNodeSet();
                nodeFileObj.Nodes.AddRange(nodes);
                Console.WriteLine($"{key}: prepped {nodes.Count} nodes: {nodeFileObj.CalculateSize()}");
                var byteArray = nodeFileObj.ToByteArray();

                var fileName = $"nodes/{key.Substring(0, 2)}/{key}";
                await rawDataUploader.WriteBlobAsync(fileName, byteArray);
            }
        }

        private static IEnumerable<(string, List<LookupNode>)> CreateProtobufs()
        {
            var folder = GlobalSettings.RouteCleanerSettings.GetInstance().TemporaryNodeWithContainingWayOutLocation;
            var allFiles = Directory.GetFiles(folder);
            foreach (var file in allFiles)
            {
                Console.WriteLine($"Working on {file}");
                var outputs = new Dictionary<string, List<LookupNode>>();
                string line;
                var sr = new StreamReader(file);
                while ((line = sr.ReadLine()) != null)
                {
                    var node = JsonConvert.DeserializeObject<Node>(line);
                    var location = new OpenLocationCode(node.Latitude, node.Longitude, codeLength: 6);
                    if (!outputs.ContainsKey(location.Code))
                    {
                        outputs.Add(location.Code, new List<LookupNode>());
                    }

                    var lNode = new LookupNode
                    {
                        Id = node.Id,
                        Latitude = node.Latitude,
                        Longitude = node.Longitude
                    };
                    lNode.Relations.AddRange(node.Relations);
                    lNode.TargetableWays.AddRange(node.ContainingWays);
                    outputs[location.Code].Add(lNode);
                }

                foreach (var kvp in outputs)
                {
                    kvp.Value.Sort(SortNodesByLatLong);
                    yield return (kvp.Key, kvp.Value);
                }
            }
        }

        private static int SortNodesByLatLong(LookupNode l, LookupNode r)
        {
            if (l.Latitude < r.Latitude)
            {
                return -1;
            }
            if (l.Latitude > r.Latitude)
            {
                return 1;
            }
            if (l.Longitude < r.Longitude)
            {
                return -1;
            }
            if (l.Longitude > r.Longitude)
            {
                return 1;
            }
            return 0;
        }
    }
}
