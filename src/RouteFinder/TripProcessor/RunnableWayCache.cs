using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureBlobHandler;
using GlobalSettings;
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
        public Dictionary<string, FullNodeSet> Cache { get; init; }

        public RunnableWayCache()
        {
            Cache = new Dictionary<string, FullNodeSet>();
        }

        /// <summary>
        /// Retrieve a segment from remote storage.
        /// </summary>
        /// <param name="plusCode"></param>
        /// <returns>true if cache miss</returns>
        public async Task<bool> LoadSegmentAsync(string plusCode)
        {
            if (Cache.ContainsKey(plusCode))
            {
                return false;
            }

            await DownloadAndCacheAsync(plusCode);
            return true;
        }

        // todo - purge if too big.
        private async Task DownloadAndCacheAsync(string plusCode)
        {
            var config = SettingsManager.GetCredentials();
            var rawDataDownloader = new DataDownloadWrapper(config.AzureRawXmlDownloadConnectionString, config.AzureBlobProcessedNodesContainer);

            var fileName = $"/nodes/{plusCode.Substring(0, 2)}/{plusCode}";
            Console.WriteLine($"Downloading cached file {fileName}");
            var localName = $"/tmp/{plusCode}";
            await rawDataDownloader.RetrieveBlobAsync(fileName, localName);

            FullNodeSet area;
            using var input = File.OpenRead(localName);
            area = FullNodeSet.Parser.ParseFrom(input);

            this.Cache.Add(plusCode, area);
        }
    }
}
