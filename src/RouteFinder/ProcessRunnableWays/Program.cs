using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureBlobHandler;
using RouteCleaner;

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

            await rawDataDownloader.RetrieveBlobAsync(tmpRemoteBoundaries, "/tmp/boundaries.xml");
            await rawDataDownloader.RetrieveBlobAsync(tmpRemoteRunnableWays, "/tmp/runnableWays.xml");

            new RouteFinderDataPrepDriver().RunChain("/tmp/boundaries.xml", "/tmp/runnableWays.xml");
            new NodeContainingWaysDriver().ProcessNodes();

            // search in the right directory and discover all files. Create dir from remoteRunnableWays then upload all files there.
            var rawDataUploader = new RawDataUploader(config.AzureRawXmlDownloadConnectionString, config.AzureBlobProcessedNodesContainer);
            var localFolder = GlobalSettings.RouteCleanerSettings.GetInstance().TemporaryNodeWithContainingWayOutLocation;
            var remoteFolder = $"/processedRunnableWays/{tmpRemoteRunnableWays.Split("/").Last().Replace(".xml", "")}";
            foreach (var fileName in Directory.GetFiles(localFolder))
            {
                var remoteFile = $"remoteFolder/{fileName}";
                Console.WriteLine($"Writing {fileName} to {remoteFile}");
                await rawDataUploader.WriteBlobAsync(remoteFile, fileName);
            }
        }

    }
}
