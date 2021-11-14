using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using AzureBlobHandler;
using OsmETL;
using RouteFinderDataModel.Proto;

namespace TripProcessor
{
    /// <summary>
    /// Cache of downloaded runnableway downloads.
    /// 
    /// Runnable ways are stored in 6-char plus code segments. This will manage downloading them and storing local caches up to a specified limit.
    /// </summary>
    public class RunnableWayCache
    {
        // if running in Azure Function, this cache will be used by multiple threads. 
        public ConcurrentDictionary<string, FullNodeSet> NodeCache { get; init; }

        public ConcurrentDictionary<string, FullWaySet> WayCache { get; init; }

        public RunnableWayCache()
        {
            NodeCache = new ConcurrentDictionary<string, FullNodeSet>();
            WayCache = new ConcurrentDictionary<string, FullWaySet>();
        }

        /// <summary>
        /// Retrieve a segment from remote storage.
        /// </summary>
        /// <param name="plusCode"></param>
        /// <returns>true if cache miss</returns>
        public Task LoadSegment(string plusCode)
        {
            if (NodeCache.ContainsKey(plusCode) && WayCache.ContainsKey(plusCode))
            {
                return Task.CompletedTask;
            }

            return DownloadAndCache(plusCode);
        }

        // todo - purge if too big.
        private Task DownloadAndCache(string plusCode)
        {
            var config = SettingsManager.GetCredentials();
            var rawDataDownloader = new DataDownloadWrapper(config.AzureRawXmlDownloadConnectionString, config.AzureBlobProcessedNodesContainer);

            var localNodeName = $"/tmp/{plusCode}";
            var localWayName = $"/tmp/way{plusCode}";

            var t1 = Task.Run(async () => {
                var fileName = $"/nodes/{plusCode[..2]}/{plusCode}";
                Console.WriteLine($"Downloading cached file {fileName}");
                await rawDataDownloader.RetrieveBlobAsync(fileName, localNodeName);

                FullNodeSet area;
                using var input = File.OpenRead(localNodeName);
                area = FullNodeSet.Parser.ParseFrom(input);

                NodeCache.TryAdd(plusCode, area);
            });

            var t2 = Task.Run(async () => {
                var fileName = $"/ways/{plusCode[..2]}/{plusCode}";
                Console.WriteLine($"Downloading cached file {fileName}");
                await rawDataDownloader.RetrieveBlobAsync(fileName, localWayName);

                FullWaySet area;
                using var input = File.OpenRead(localWayName);
                area = FullWaySet.Parser.ParseFrom(input);

                WayCache.TryAdd(plusCode, area);
            });

            return Task.WhenAll(t1, t2);
        }
    }
}
