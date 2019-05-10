using OpenStreetMapEtl.Utils;
using RouteCleaner;
using RouteCleaner.Model;
using System;

namespace OpenStreetMapEtl
{
    /// <summary>
    /// Orchestrate the download and cleaning process.
    /// </summary>
    public class DownloaderGeneral
    {
        private readonly double _kmSize = 16;

        private readonly IRangeDownloader _downloader = new OsmDownloader();

        private readonly DetailedDebugOutputter _debugger = new DetailedDebugOutputter("C:/tmp");

        public DownloaderGeneral(IRangeDownloader downloader)
        {
            _downloader = downloader;
        }

        public void Run(double westLng, double eastLng, double southLat, double northLat)
        {
            GetRange(westLng, eastLng, southLat, northLat, out var latDelta, out var lonDelta);
            LogRanges(latDelta, lonDelta);

            var localSouthLat = southLat;
            while (localSouthLat < northLat)
            {
                var localNorthLat = Math.Min(localSouthLat + latDelta, northLat);
                if (northLat - localNorthLat < 0.05)
                {
                    localNorthLat = northLat;
                }
                var localWestLng = westLng;
                while (localWestLng < eastLng)
                {
                    var localEastLng = Math.Min(localWestLng + lonDelta, eastLng);
                    localEastLng = localEastLng - eastLng < 0.05 ? eastLng : localEastLng;
                    RunSingleSquare(localWestLng, localEastLng, localSouthLat, localNorthLat);
                    localWestLng = localEastLng;
                }
                localSouthLat = localNorthLat;
            }
        }

        private Geometry RunSingleSquare(double westLng, double eastLng, double southLat, double northLat)
        {
            using (var downloadedFile = _downloader.GetRange(westLng, eastLng, southLat, northLat))
            {
                var cleaner = new ParseAndCleanOsm();
                var geometry = cleaner.ReadAndClean(downloadedFile.TmpFile);
                var parkingLots = new PathParkingLotIntersection().FindParkingLotsWithIntersections(geometry);
                _debugger.OutputWays(parkingLots.Keys, "parkingLots.json");
                return geometry;
            }
        }

        private void GetRange(double westLng, double eastLng, double southLat, double northLat, out double latDelta, out double lonDelta)
        {
            latDelta = (northLat - southLat) / SimpleDistanceCost.Compute(northLat, southLat, westLng, westLng) * _kmSize;
            lonDelta = (eastLng - westLng) / SimpleDistanceCost.Compute(northLat, northLat, eastLng, westLng) * _kmSize;
        }

        private void LogRanges(double latDelta, double lngDelta)
        {
            Console.WriteLine($"Deltas: {latDelta},{lngDelta}");
        }
    }
}
