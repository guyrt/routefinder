
namespace OsmETL
{
    using System.IO;
    using CosmosDBLayer;
    using OsmDataLoader;
    using RouteCleaner;
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

            // build the classifier
            var osmDeserializer = new OsmDeserializer();
            Geometry originalGeometry;
            using (var fs = File.OpenRead(localFile))
            {
                using (var sr = new StreamReader(fs))
                {
                    originalGeometry = osmDeserializer.ReadFile(sr);
                }
            }
            
        }
    }
}
