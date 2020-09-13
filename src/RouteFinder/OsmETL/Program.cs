
namespace OsmETL
{
    using System.IO;
    using OsmDataLoader;
    using RouteCleaner;
    using RouteFinderDataModel;

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
            var numnodes = originalGeometry.Nodes.Length;
        }
    }
}
