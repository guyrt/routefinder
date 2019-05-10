using OpenStreetMapEtl.Utils;
using System;
using System.IO;
using System.Net;

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

        public TmpFileWrapper GetRange(double westLng, double eastLng, double southLat, double northLat)
        {
            var path = $"{_basePath}?bbox={westLng},{southLat},{eastLng},{northLat}";
            LogPath(path);
            var tmpFile = new TmpFileWrapper();
            var webClient = new WebClient();
            webClient.DownloadFile(path, tmpFile.TmpFile);
            return tmpFile;
        }

        private void LogPath(string path)
        {
            Console.WriteLine($"Path: {path}");
        }
        
    }
}
