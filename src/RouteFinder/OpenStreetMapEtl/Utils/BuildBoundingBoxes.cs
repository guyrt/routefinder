using RouteCleaner;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenStreetMapEtl.Utils
{
    public class BuildBoundingBoxes
    {
        private readonly double _kmSize;

        public BuildBoundingBoxes(double kmSize)
        {
            _kmSize = kmSize;
        }

        public IEnumerable<BoundingBox> GetBoundingBoxes(BoundingBox originalBox)
        {
            var westLng = originalBox.WestLongitude;
            var eastLng = originalBox.EastLongitude;
            var northLat = originalBox.NorthLatitude;
            var southLat = originalBox.SouthLatitude;
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
                    localEastLng = eastLng - localEastLng < 0.05 ? eastLng : localEastLng;
                    yield return new BoundingBox { EastLongitude = localEastLng, WestLongitude = localWestLng, NorthLatitude = localNorthLat, SouthLatitude = localSouthLat };
                    localWestLng = localEastLng;
                }
                localSouthLat = localNorthLat;
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
