using RouteFinderDataModel;
using System;
using System.Diagnostics;
using System.IO;

namespace RouteCleaner
{
    public static class GeometryFactory
    {
        public static Geometry GetRegionGeometry(string filePath, bool ignoreNodes, bool trimTags)
        {
            var watch = Stopwatch.StartNew();
            var osmDeserializer = new OsmDeserializer(true);
            Geometry relationRegion;
            using (var fs = File.OpenRead(filePath))
            {
                using (var sr = new StreamReader(fs))
                {
                    Console.WriteLine($"Loading regions from {filePath}.");
                    relationRegion = osmDeserializer.ReadFile(sr, ignoreNodes, trimTags);
                }
            }
            var time = watch.Elapsed;
            Console.WriteLine($"Done loading {filePath} in {time}");
            return relationRegion;
        }
    }
}
