using OpenStreetMapEtl.Utils;
using System;
using System.Net;
using System.Threading;

namespace OpenStreetMapEtl
{
    /// <summary>
    /// Download the raw OpenStreetMap files.
    /// </summary>
    public class OsmDownloader : IRangeDownloader
    {
        private string _basePath;

        public OsmDownloader() : this("https://overpass-api.de/api/map") { }

        public OsmDownloader(string basePath)
        {
            _basePath = basePath;
        }

        public TmpFileWrapper GetRange(BoundingBox box)
        {
            var retries = 0;
            while (retries < 3)
            {
                var path = $"{_basePath}?bbox={box.WestLongitude},{box.SouthLatitude},{box.EastLongitude},{box.NorthLatitude}";
                LogPath(path);
                var tmpFile = new TmpFileWrapper();
                var webClient = new WebClient();
                try
                {
                    webClient.DownloadFile(path, tmpFile.TmpFile);
                    return tmpFile;

                }
                catch (WebException)
                {
                    retries += 1;
                    Thread.Sleep(1000 * 60 * 10);  // sleep for 10 minutes.
                }
            }
            throw new InvalidOperationException($"Unable to download after 3 tries");
        }

        private void LogPath(string path)
        {
            Console.WriteLine($"Path: {path}");
        }
        
    }
}
