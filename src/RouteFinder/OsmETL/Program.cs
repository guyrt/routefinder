
namespace OsmETL
{
    using System.IO;
    using CosmosDBLayer;
    using OsmDataLoader;
    using RouteCleaner;
    using RouteCleaner.Transformers;
    using RouteFinderDataModel;

    public class Program
    {
        static void Main(string[] args)
        {
            var config = SettingsManager.GetCredentials();

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

            var uploadHandler = new UploadHandler(config.EndPoint, config.AuthKey);
            uploadHandler.Upload(ways);
        }
    }
}
