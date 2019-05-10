using RouteCleaner;
using RouteCleaner.Model;
using RouteCleaner.Transformers;
using System;
using System.Linq;
using System.IO;


namespace OpenStreetMapEtl
{
    internal class ParseAndCleanOsm
    {
        public Geometry ReadAndClean(string fileName)
        {
            var deserializer = new OsmDeserializer();
            var fileHandle = File.OpenText(fileName);
            var geometry = deserializer.ReadFile(fileHandle);
            fileHandle.Close();
            LogSize(geometry, "raw");
            // strip parking lot aisles
            geometry = new DropParkingAisle().Transform(geometry);
            LogSize(geometry, "parkingaisle");
            // strip buildings
            geometry = new DropBuildings().Transform(geometry);
            LogSize(geometry, "buildings");
            // strip city boundaries
            geometry = new DropMunicipalBoundaries().Transform(geometry);
            LogSize(geometry, "boundaries");
            // strip city boundaries
            geometry = new DropTrees().Transform(geometry);
            LogSize(geometry, "trees");
            // strip tiger
            geometry = new DropTigerTags().Transform(geometry);
            LogSize(geometry, "tiger");
            // strip driveways
            geometry = new DropDriveways().Transform(geometry);
            LogSize(geometry, "driveways");
            // strip underground (lol)
            geometry = new DropUnderground().Transform(geometry);
            LogSize(geometry, "underground");
            return geometry;
        }

        private void LogSize(Geometry geometry, string operation)
        {
            var numTags = geometry.Nodes.Sum(n => n.Tags.Count) + geometry.Ways.Sum(n => n.Tags.Count) + geometry.Relations.Sum(n => n.Tags.Count);
            Console.WriteLine($"{operation},{geometry.Nodes.Length},{geometry.Ways.Length},{geometry.Relations.Length},{numTags}");
        }
    }
}
