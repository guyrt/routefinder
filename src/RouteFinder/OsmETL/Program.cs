namespace OsmETL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using CosmosDBLayer;
    using Google.OpenLocationCode;
    using Newtonsoft.Json;
    using RouteFinderDataModel;
    using RouteFinderDataModel.Proto;

    public class Program
    {
        public static void Main(string[] args)
        {
            var config = SettingsManager.GetCredentials();

            foreach ((var key, var nodes) in CreateProtobufs())
            {
                var nodeFileObj = new FullNodeSet();
                nodeFileObj.Nodes.AddRange(nodes);
                Console.WriteLine($"{key}: prepped {nodes.Count} nodes: {nodeFileObj.CalculateSize()}");

                // todo: create byte stream
                // todo: upload!
            }


            // everything below here is old
            /*            var uploadHandler = new UploadHandler(config.EndPoint, config.AuthKey, config.CosmosDatabase, config.CosmosContainer);

                        var configObj = new DownloadConfig()
                        {
                            LocalFilePattern = config.LocalFilePattern,
                            LocalBz2FilePattern = config.LocalBz2FilePattern,
                            RemoteFile = config.RemoteFile,
                            RemoteMd5File = config.RemoteMd5File
                        };
                        var downloader = new DownloadManager(configObj);
                        var localFile = downloader.DownloadAndUnzip();

                        // build the dataset
                        var osmDeserializer = new OsmDeserializer();
                        Geometry region;
                        using (var fs = File.OpenRead(localFile))
                        {
                            using (var sr = new StreamReader(fs))
                            {
                                region = osmDeserializer.ReadFile(sr);
                            }
                        }

                        // clean data
                        region = new CollapseParkingLots().Transform(region);

                        // todo get interesting sets of ways and nodes that we might augment to later.

                        // Get final set of ways
                        region = new OnlyTraversable().Transform(region);
                        var ways = new SplitBisectedWays().Transform(region.Ways);

                        await uploadHandler.Upload(ways);*/
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
