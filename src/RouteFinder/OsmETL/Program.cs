
namespace OsmETL
{
    using System;
    using System.IO;
    using OsmDataLoader;
    using RouteCleaner;
    using RouteCleaner.Model;
    using RouteCleaner.Transformers;

    public class Program
    {
        static void Main(string[] args)
        {
            var configObj = new DownloadConfig()
            {
                LocalFilePattern = "C:/tmp/washington.{0}.osm",
                LocalBz2FilePattern = "C:/tmp/washington.{0}.osm.bz2",
                RemoteFile = "http://download.geofabrik.de/north-america/us/washington-latest.osm.bz2",
                RemoveMd5File = "http://download.geofabrik.de/north-america/us/washington-latest.osm.bz2.md5"
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
            var way = ways[0];
        }
    }
}
